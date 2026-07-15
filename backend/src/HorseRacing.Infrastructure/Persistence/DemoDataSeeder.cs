using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Domain.Entities.Financials;

namespace HorseRacing.Infrastructure.Persistence;

public class DemoUserSeedDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

public class DemoDataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DemoDataSeeder> _logger;

    public DemoDataSeeder(AppDbContext context, ILogger<DemoDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting rich developer/test demo data seeding...");

        try
        {
            var hasher = new PasswordHasher<AppUser>();
            var fixedDate = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc);

            _logger.LogInformation("Wiping old demo data to prepare a fresh clean slate...");

            // Order of deletion to avoid FK violations:
            _context.Predictions.RemoveRange(_context.Predictions);
            _context.RefereeReports.RemoveRange(_context.RefereeReports);
            _context.Violations.RemoveRange(_context.Violations);
            _context.RaceResults.RemoveRange(_context.RaceResults);
            _context.RaceRefereeAssignments.RemoveRange(_context.RaceRefereeAssignments);
            _context.RaceEntries.RemoveRange(_context.RaceEntries);
            _context.JockeyContracts.RemoveRange(_context.JockeyContracts);
            _context.MedicalCheckRecords.RemoveRange(_context.MedicalCheckRecords);
            _context.Registrations.RemoveRange(_context.Registrations);
            _context.Races.RemoveRange(_context.Races);
            _context.Rounds.RemoveRange(_context.Rounds);
            _context.Tournaments.RemoveRange(_context.Tournaments);
            _context.HorseDocuments.RemoveRange(_context.HorseDocuments);
            _context.HorseStatistics.RemoveRange(_context.HorseStatistics);
            _context.Horses.RemoveRange(_context.Horses);

            _context.Payouts.RemoveRange(_context.Payouts);
            _context.Bets.RemoveRange(_context.Bets);
            _context.TournamentPrizePayouts.RemoveRange(_context.TournamentPrizePayouts);
            _context.Prizes.RemoveRange(_context.Prizes);
            _context.Transactions.RemoveRange(_context.Transactions);
            _context.Notifications.RemoveRange(_context.Notifications);

            _context.RefereeProfiles.RemoveRange(_context.RefereeProfiles);
            _context.JockeyProfiles.RemoveRange(_context.JockeyProfiles);
            _context.Wallets.RemoveRange(_context.Wallets);

            // Remove all users except admin@gmail.com
            var nonAdminUsers = _context.Users.Where(u => u.Email != "admin@gmail.com");
            _context.Users.RemoveRange(nonAdminUsers);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Wipe complete. Starting clean seeding of new test data...");

            // 1. Seed Demo Users & Wallets & Profiles
            var demoUsersList = new List<DemoUserSeedDto>
            {
                new() { Username = "owner",     Email = "owner@gmail.com",     FullName = "John Nguyen", RoleId = 2 },
                new() { Username = "owner2",    Email = "owner2@gmail.com",    FullName = "Mary Tran",     RoleId = 2 },
                new() { Username = "owner3",    Email = "owner3@gmail.com",    FullName = "David Le",     RoleId = 2 },
                new() { Username = "jockey",    Email = "jockey@gmail.com",    FullName = "Jockey Nguyen",    RoleId = 3 },
                new() { Username = "referee",   Email = "referee@gmail.com",   FullName = "Referee Nam",    RoleId = 4 },
                new() { Username = "spectator", Email = "spectator@gmail.com", FullName = "Spectator Binh",    RoleId = 5 },
                new() { Username = "spectator2", Email = "spectator2@gmail.com", FullName = "Spectator Hoang",   RoleId = 5 },
                new() { Username = "spectator3", Email = "spectator3@gmail.com", FullName = "Spectator Dung",    RoleId = 5 },
                new() { Username = "vet",        Email = "vet@gmail.com",        FullName = "Veterinarian",     RoleId = 6 }
            };

            for (int i = 1; i <= 10; i++)
            {
                demoUsersList.Add(new DemoUserSeedDto { Username = $"jockey{i}", Email = $"jockey{i}@gmail.com", FullName = $"Jockey #{i}", RoleId = 3 });
            }

            foreach (var item in demoUsersList)
            {
                if (!await _context.Users.AnyAsync(u => u.Username == item.Username || u.Email == item.Email))
                {
                    var user = new AppUser
                    {
                        Username = item.Username,
                        Email = item.Email,
                        FullName = item.FullName,
                        RoleId = item.RoleId,
                        Status = "Active",
                        CreatedAt = fixedDate
                    };
                    user.PasswordHash = hasher.HashPassword(user, "123456");
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    if (item.RoleId == 3) // Jockey
                    {
                        _context.JockeyProfiles.Add(new JockeyProfile
                        {
                            UserId = user.UserId,
                            ExperienceYears = 3 + (user.UserId % 7),
                            RankingPoint = 100 + (user.UserId % 100),
                            Status = "Active"
                        });
                    }
                    else if (item.RoleId == 4) // Referee
                    {
                        _context.RefereeProfiles.Add(new RefereeProfile
                        {
                            UserId = user.UserId,
                            LicenseNumber = $"LIC-REF-{user.UserId:D3}",
                            ExperienceYears = 7,
                            Status = "Active"
                        });
                    }

                    var initialBalance = item.RoleId == 5 ? 50000.00m : 10000.00m;
                    _context.Wallets.Add(new Wallet
                    {
                        UserId = user.UserId,
                        Balance = initialBalance
                    });

                    await _context.SaveChangesAsync();
                }
            }

            // Seed 25 Jockeys dynamically
            for (int i = 1; i <= 25; i++)
            {
                var jockeyUsername = $"jockey{i}";
                var jockeyEmail = $"jockey{i}@gmail.com";
                if (!await _context.Users.AnyAsync(u => u.Username == jockeyUsername || u.Email == jockeyEmail))
                {
                    var user = new AppUser
                    {
                        Username = jockeyUsername,
                        Email = jockeyEmail,
                        FullName = $"Jockey #{i}",
                        RoleId = 3, // Jockey
                        Status = "Active",
                        CreatedAt = fixedDate
                    };
                    user.PasswordHash = hasher.HashPassword(user, "123456");
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _context.JockeyProfiles.Add(new JockeyProfile
                    {
                        UserId = user.UserId,
                        ExperienceYears = 2 + (i % 4),
                        Status = "Active"
                    });
                    _context.Wallets.Add(new Wallet
                    {
                        UserId = user.UserId,
                        Balance = 1000m
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Seed 20 Referees dynamically
            for (int i = 1; i <= 20; i++)
            {
                var refUsername = $"referee{i}";
                var refEmail = $"referee{i}@gmail.com";
                if (!await _context.Users.AnyAsync(u => u.Username == refUsername || u.Email == refEmail))
                {
                    var user = new AppUser
                    {
                        Username = refUsername,
                        Email = refEmail,
                        FullName = $"Referee #{i}",
                        RoleId = 4,
                        Status = "Active",
                        CreatedAt = fixedDate
                    };
                    user.PasswordHash = hasher.HashPassword(user, "123456");
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _context.RefereeProfiles.Add(new RefereeProfile
                    {
                        UserId = user.UserId,
                        LicenseNumber = $"LIC-REF-{i:D3}",
                        ExperienceYears = 3 + (i % 5),
                        Status = "Active"
                    });
                    _context.Wallets.Add(new Wallet
                    {
                        UserId = user.UserId,
                        Balance = 1000m
                    });
                    await _context.SaveChangesAsync();
                }
            }

            var ownerUser = await _context.Users.FirstAsync(u => u.Username == "owner");
            var owner2User = await _context.Users.FirstAsync(u => u.Username == "owner2");
            var owner3User = await _context.Users.FirstAsync(u => u.Username == "owner3");
            var jockeys = await _context.JockeyProfiles.Include(jp => jp.User).ToListAsync();
            var refereeProfile = await _context.RefereeProfiles.Include(rp => rp.User).FirstAsync(rp => rp.User.Username == "referee");
            var jockeyUser = await _context.Users.FirstAsync(u => u.Username == "jockey");
            var jockeyProf = await _context.JockeyProfiles.FirstAsync(jp => jp.UserId == jockeyUser.UserId);
            var refereeUser = await _context.Users.FirstAsync(u => u.Username == "referee");
            var spectatorUser = await _context.Users.FirstAsync(u => u.Username == "spectator");
            var spectator2User = await _context.Users.FirstAsync(u => u.Username == "spectator2");
            var spectator3User = await _context.Users.FirstAsync(u => u.Username == "spectator3");
            var baseDate = fixedDate;

            // 2. Seed 30 Horses
            var horseData = new[]
            {
                new { Name = "Red Rum", Age = baseDate.AddYears(-6), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 68.50m, RecentAvgTime = 68.50m, WinRate = 0.25m },
                new { Name = "Secretariat", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 67.20m, RecentAvgTime = 67.20m, WinRate = 0.80m },
                new { Name = "Seattle Slew", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 70.00m, RecentAvgTime = 70.00m, WinRate = 0.50m },
                new { Name = "Spectacular Bid", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 67.90m, RecentAvgTime = 67.90m, WinRate = 0.65m },
                new { Name = "Zenyatta", Age = baseDate.AddYears(-6), Gender = "Mare", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 69.00m, RecentAvgTime = 69.00m, WinRate = 0.85m },

                new { Name = "Shergar", Age = baseDate.AddYears(-6), Gender = "Colt", Breed = "Irish Draught", OwnerId = owner2User.UserId, AvgTime = 71.10m, RecentAvgTime = 71.10m, WinRate = 0.40m },
                new { Name = "Eclipse", Age = baseDate.AddYears(-7), Gender = "Gelding", Breed = "Arabian", OwnerId = owner2User.UserId, AvgTime = 69.80m, RecentAvgTime = 69.80m, WinRate = 0.35m },
                new { Name = "Affirmed", Age = baseDate.AddYears(-6), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 69.50m, RecentAvgTime = 69.50m, WinRate = 0.45m },
                new { Name = "Ruffian", Age = baseDate.AddYears(-4), Gender = "Filly", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 67.50m, RecentAvgTime = 67.50m, WinRate = 0.90m },
                new { Name = "Frankel", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 66.80m, RecentAvgTime = 66.80m, WinRate = 0.95m },

                new { Name = "Man o' War", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 68.00m, RecentAvgTime = 68.00m, WinRate = 0.75m },
                new { Name = "Phar Lap", Age = baseDate.AddYears(-6), Gender = "Gelding", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 69.10m, RecentAvgTime = 69.10m, WinRate = 0.60m },
                new { Name = "Citation", Age = baseDate.AddYears(-7), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 68.90m, RecentAvgTime = 68.90m, WinRate = 0.70m },
                new { Name = "War Admiral", Age = baseDate.AddYears(-6), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 68.40m, RecentAvgTime = 68.40m, WinRate = 0.70m },

                new { Name = "Seabiscuit", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 68.00m, RecentAvgTime = 68.00m, WinRate = 0.60m },
                new { Name = "Black Caviar", Age = baseDate.AddYears(-4), Gender = "Mare", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 65.50m, RecentAvgTime = 65.50m, WinRate = 0.99m },
                new { Name = "Winx", Age = baseDate.AddYears(-5), Gender = "Mare", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 66.00m, RecentAvgTime = 66.00m, WinRate = 0.98m },
                new { Name = "Makybe Diva", Age = baseDate.AddYears(-6), Gender = "Mare", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 68.20m, RecentAvgTime = 68.20m, WinRate = 0.75m },
                new { Name = "Arrogate", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 67.00m, RecentAvgTime = 67.00m, WinRate = 0.80m },

                new { Name = "Gun Runner", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 67.50m, RecentAvgTime = 67.50m, WinRate = 0.75m },
                new { Name = "Justify", Age = baseDate.AddYears(-4), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 66.20m, RecentAvgTime = 66.20m, WinRate = 0.95m },
                new { Name = "American Pharoah", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 66.50m, RecentAvgTime = 66.50m, WinRate = 0.90m },
                new { Name = "California Chrome", Age = baseDate.AddYears(-6), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 67.80m, RecentAvgTime = 67.80m, WinRate = 0.80m },
                new { Name = "Shared Belief", Age = baseDate.AddYears(-5), Gender = "Gelding", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 68.10m, RecentAvgTime = 68.10m, WinRate = 0.70m },

                new { Name = "Barbaro", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 67.40m, RecentAvgTime = 67.40m, WinRate = 0.85m },
                new { Name = "Smarty Jones", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 67.60m, RecentAvgTime = 67.60m, WinRate = 0.80m },
                new { Name = "Funny Cide", Age = baseDate.AddYears(-6), Gender = "Gelding", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 68.30m, RecentAvgTime = 68.30m, WinRate = 0.70m },
                new { Name = "Sunday Silence", Age = baseDate.AddYears(-7), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 67.90m, RecentAvgTime = 67.90m, WinRate = 0.80m },
                new { Name = "Easy Goer", Age = baseDate.AddYears(-7), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 67.70m, RecentAvgTime = 67.70m, WinRate = 0.78m },

                // Custom horses for testing
                new { Name = "Blaze", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 65.40m, RecentAvgTime = 65.30m, WinRate = 0.40m },
                new { Name = "Thunder", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = ownerUser.UserId, AvgTime = 66.38m, RecentAvgTime = 66.83m, WinRate = 0.20m },
                new { Name = "Comet", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 65.94m, RecentAvgTime = 65.87m, WinRate = 0.00m },
                new { Name = "Wind Ranger", Age = baseDate.AddYears(-6), Gender = "Mare", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 70.52m, RecentAvgTime = 70.63m, WinRate = 0.00m },
                new { Name = "Dusty", Age = baseDate.AddYears(-6), Gender = "Mare", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 71.38m, RecentAvgTime = 70.97m, WinRate = 0.00m },
                new { Name = "Rusty", Age = baseDate.AddYears(-7), Gender = "Gelding", Breed = "Arabian", OwnerId = owner3User.UserId, AvgTime = 76.20m, RecentAvgTime = 75.43m, WinRate = 0.00m },
                new { Name = "Tortoise", Age = baseDate.AddYears(-6), Gender = "Gelding", Breed = "Arabian", OwnerId = ownerUser.UserId, AvgTime = 78.14m, RecentAvgTime = 78.30m, WinRate = 0.00m },
                new { Name = "Rising Star", Age = baseDate.AddYears(-4), Gender = "Filly", Breed = "Thoroughbred", OwnerId = owner2User.UserId, AvgTime = 70.36m, RecentAvgTime = 68.33m, WinRate = 0.20m },
                new { Name = "Wild Wind", Age = baseDate.AddYears(-5), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 70.64m, RecentAvgTime = 70.57m, WinRate = 0.20m }
            };

            foreach (var h in horseData)
            {
                if (!await _context.Horses.AnyAsync(horse => horse.Name == h.Name))
                {
                    _context.Horses.Add(new Horse
                    {
                        Name = h.Name,
                        Age = h.Age,
                        Gender = h.Gender,
                        Breed = h.Breed,
                        HealthStatus = "Healthy",
                        OwnerId = h.OwnerId,
                        AverageTime = h.AvgTime,
                        RecentAverageTime = h.RecentAvgTime,
                        WinRate = h.WinRate
                    });
                }
            }
            await _context.SaveChangesAsync();
            var allHorses = await _context.Horses.ToListAsync();

            // 3. Seed 5 Completed Tournaments
            var completedTournaments = new List<(string Name, DateTime Start, DateTime End, string RaceName, int WinnerIndex, decimal[] Times, int[] Positions)>
            {
                (
                    "Spring Horse Racing Tournament 2026", 
                    new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    "Spring Grand Battle",
                    0, // Blaze wins
                    new decimal[] { 65.20m, 65.80m, 66.00m, 70.20m, 71.00m, 76.50m, 78.00m, 74.00m, 71.50m },
                    new int[] { 1, 2, 3, 4, 5, 8, 9, 7, 6 }
                ),
                (
                    "Royal Cup Tournament 2026", 
                    new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                    "Annual Royal Cup",
                    1, // Thunder wins
                    new decimal[] { 65.90m, 65.60m, 66.10m, 70.50m, 73.00m, 78.20m, 77.80m, 72.80m, 70.00m },
                    new int[] { 2, 1, 3, 5, 7, 9, 8, 6, 4 }
                ),
                (
                    "Hanoi Derby Tournament 2026", 
                    new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
                    "Hanoi Derby Race",
                    8, // Wild Wind wins
                    new decimal[] { 65.10m, 66.00m, 65.90m, 71.80m, 71.20m, 75.80m, 78.50m, 71.00m, 65.00m },
                    new int[] { 2, 4, 3, 7, 6, 8, 9, 5, 1 }
                ),
                (
                    "National Championship Tournament 2026", 
                    new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                    "National Finals",
                    0, // Blaze wins
                    new decimal[] { 64.90m, 68.50m, 65.50m, 70.10m, 70.90m, 75.50m, 78.10m, 68.20m, 77.20m },
                    new int[] { 1, 4, 2, 5, 6, 7, 9, 3, 8 }
                ),
                (
                    "Autumn Super Cup Tournament 2026", 
                    new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc),
                    "Autumn Super Cup",
                    7, // Rising Star wins
                    new decimal[] { 65.90m, 66.00m, 66.20m, 70.00m, 70.80m, 75.00m, 78.30m, 65.80m, 69.50m },
                    new int[] { 2, 3, 4, 6, 7, 8, 9, 1, 5 }
                )
            };

            var customHorseNames = new[] { "Blaze", "Thunder", "Comet", "Wind Ranger", "Dusty", "Rusty", "Tortoise", "Rising Star", "Wild Wind" };
            var customHorses = allHorses.Where(h => customHorseNames.Contains(h.Name)).OrderBy(h => Array.IndexOf(customHorseNames, h.Name)).ToList();
            var allJockeys = await _context.JockeyProfiles.ToListAsync();

            foreach (var ct in completedTournaments)
            {
                var t = await _context.Tournaments.FirstOrDefaultAsync(x => x.Name == ct.Name);
                if (t == null)
                {
                    t = new Tournament
                    {
                        Name = ct.Name,
                        StartDate = ct.Start,
                        EndDate = ct.End,
                        Status = "Completed"
                    };
                    _context.Tournaments.Add(t);
                    await _context.SaveChangesAsync();

                    var round = new Round
                    {
                        TournamentId = t.TournamentId,
                        Name = "Finals",
                        RoundNumber = 1,
                        StartDate = ct.Start,
                        EndDate = ct.End,
                        Status = "Completed"
                    };
                    _context.Rounds.Add(round);
                    await _context.SaveChangesAsync();

                    var race = new Race
                    {
                        RoundId = round.RoundId,
                        Name = ct.RaceName,
                        RaceDate = ct.Start.AddDays(4),
                        DistanceMeter = 1200,
                        MaxLanes = 10,
                        Status = "Completed"
                    };
                    _context.Races.Add(race);
                    await _context.SaveChangesAsync();

                    var entriesList = new List<RaceEntry>();
                    for (int i = 0; i < customHorses.Count; i++)
                    {
                        var horse = customHorses[i];
                        
                        var reg = new Registration
                        {
                            TournamentId = t.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Approved",
                            RegisteredAt = ct.Start.AddDays(-5)
                        };
                        _context.Registrations.Add(reg);
                        await _context.SaveChangesAsync();

                        var jockey = allJockeys[i % allJockeys.Count];
                        var contract = new JockeyContract
                        {
                            TournamentId = t.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = ct.Start,
                            EndDate = ct.End,
                            Status = "Accepted",
                            CreatedAt = ct.Start.AddDays(-4)
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();

                        var entry = new RaceEntry
                        {
                            RaceId = race.RaceId,
                            RegistrationId = reg.RegistrationId,
                            JockeyId = jockey.JockeyId,
                            LaneNo = i + 1,
                            Status = "Finished",
                            WinningProbability = 11.1m,
                            CurrentOdds = 2.0m,
                            FinishTime = ct.Times[i],
                            FinishPosition = ct.Positions[i]
                        };
                        entriesList.Add(entry);
                    }
                    _context.RaceEntries.AddRange(entriesList);
                    await _context.SaveChangesAsync();

                    var winnerHorse = customHorses[ct.WinnerIndex];
                    _context.RaceResults.Add(new RaceResult
                    {
                        RaceId = race.RaceId,
                        Winner = winnerHorse.Name,
                        ResultRecordedAt = race.RaceDate.AddHours(1),
                        CreatedAt = race.RaceDate.AddHours(1)
                    });
                    await _context.SaveChangesAsync();

                    if (ct.Name == "Spring Horse Racing Tournament 2026")
                    {
                        var winnerEntry = entriesList.First(e => e.FinishPosition == 1);
                        var runnerUpEntry = entriesList.First(e => e.FinishPosition == 2);
                        var runnerUpHorse = customHorses[ct.WinnerIndex == 0 ? 1 : 0];

                        var bet1 = new Bet
                        {
                            UserId = spectatorUser.UserId,
                            RaceId = race.RaceId,
                            HorseId = winnerHorse.HorseId,
                            Amount = 5000m,
                            Odds = 2.5m,
                            Status = "Won",
                            CreatedAt = race.RaceDate.AddHours(-2),
                            RaceEntryId = winnerEntry.RaceEntryId
                        };
                        var bet2 = new Bet
                        {
                            UserId = spectator2User.UserId,
                            RaceId = race.RaceId,
                            HorseId = runnerUpHorse.HorseId,
                            Amount = 3000m,
                            Odds = 3.0m,
                            Status = "Lost",
                            CreatedAt = race.RaceDate.AddHours(-1.5),
                            RaceEntryId = runnerUpEntry.RaceEntryId
                        };
                        var bet3 = new Bet
                        {
                            UserId = spectator3User.UserId,
                            RaceId = race.RaceId,
                            HorseId = winnerHorse.HorseId,
                            Amount = 10000m,
                            Odds = 2.2m,
                            Status = "Won",
                            CreatedAt = race.RaceDate.AddHours(-1),
                            RaceEntryId = winnerEntry.RaceEntryId
                        };
                        _context.Bets.AddRange(bet1, bet2, bet3);
                        await _context.SaveChangesAsync();

                        _context.Payouts.Add(new Payout
                        {
                            BetId = bet1.Id,
                            Amount = 5000m * 2.5m,
                            CreatedAt = race.RaceDate.AddMinutes(5)
                        });
                        _context.Payouts.Add(new Payout
                        {
                            BetId = bet3.Id,
                            Amount = 10000m * 2.2m,
                            CreatedAt = race.RaceDate.AddMinutes(5)
                        });
                        await _context.SaveChangesAsync();

                        _context.Predictions.Add(new Prediction
                        {
                            UserId = spectatorUser.UserId,
                            RaceId = race.RaceId,
                            RaceEntryId = winnerEntry.RaceEntryId,
                            PredictedAt = race.RaceDate.AddHours(-4),
                            Status = "Evaluated",
                            IsCorrect = true,
                            Point = 100
                        });
                        _context.Predictions.Add(new Prediction
                        {
                            UserId = spectator2User.UserId,
                            RaceId = race.RaceId,
                            RaceEntryId = runnerUpEntry.RaceEntryId,
                            PredictedAt = race.RaceDate.AddHours(-3.5),
                            Status = "Evaluated",
                            IsCorrect = false,
                            Point = 0
                        });
                        _context.Predictions.Add(new Prediction
                        {
                            UserId = spectator3User.UserId,
                            RaceId = race.RaceId,
                            RaceEntryId = winnerEntry.RaceEntryId,
                            PredictedAt = race.RaceDate.AddHours(-3),
                            Status = "Evaluated",
                            IsCorrect = true,
                            Point = 100
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // 4. Seed Tournament 2: "Giải Đua Ngựa Mùa Hè 2026" (ONGOING / LIVE)
            var t2Name = "Summer Horse Racing Tournament 2026";
            var t2 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t2Name);
            if (t2 == null)
            {
                t2 = new Tournament
                {
                    Name = t2Name,
                    StartDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Ongoing"
                };
                _context.Tournaments.Add(t2);
                await _context.SaveChangesAsync();

                var t2Round = new Round
                {
                    TournamentId = t2.TournamentId,
                    Name = "Qualifier 1",
                    RoundNumber = 1,
                    StartDate = new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 6, 28, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Ongoing"
                };
                _context.Rounds.Add(t2Round);
                await _context.SaveChangesAsync();

                var t2Race = new Race
                {
                    RoundId = t2Round.RoundId,
                    Name = "Summer Opening Race",
                    RaceDate = DateTime.UtcNow.AddDays(1),
                    DistanceMeter = 1400,
                    MaxLanes = 8,
                    Status = "Ongoing"
                };
                _context.Races.Add(t2Race);
                await _context.SaveChangesAsync();

                var t2Entries = new List<RaceEntry>();
                for (int i = 0; i < customHorses.Count; i++)
                {
                    var horse = customHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t2.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
                    };
                    _context.Registrations.Add(reg);
                    await _context.SaveChangesAsync();

                    var entry = new RaceEntry
                    {
                        RaceId = t2Race.RaceId,
                        RegistrationId = reg.RegistrationId,
                        JockeyId = allJockeys[i % allJockeys.Count].JockeyId,
                        LaneNo = i + 1,
                        Status = "Ready"
                    };
                    t2Entries.Add(entry);
                }
                _context.RaceEntries.AddRange(t2Entries);
                await _context.SaveChangesAsync();

                // Calculate Odds using bookmaker algorithm
                var t2Scores = new List<(RaceEntry entry, decimal score)>();
                foreach (var entry in t2Entries)
                {
                    var horse = customHorses.First(h => h.HorseId == _context.Registrations.First(r => r.RegistrationId == entry.RegistrationId).HorseId);
                    var avg = horse.AverageTime ?? 70m;
                    var rec = horse.RecentAverageTime ?? avg;
                    var win = horse.WinRate ?? 0.05m;
                    if (win > 1) win /= 100m;

                    var avgScore = Math.Max(1m, 100m - (avg - 60m) * 5m);
                    var recScore = Math.Max(1m, 100m - (rec - 60m) * 5m);
                    var winScore = win * 100m;

                    var score = avgScore * 0.4m + recScore * 0.4m + winScore * 0.2m;
                    t2Scores.Add((entry, score));
                }
                var t2TotalScore = t2Scores.Sum(x => x.score);
                foreach (var item in t2Scores)
                {
                    var prob = item.score / t2TotalScore;
                    item.entry.WinningProbability = Math.Round(prob * 100m, 2);
                    item.entry.CurrentOdds = Math.Round(Math.Max((1m / prob) * 0.9m, 1.05m), 2);
                }
                await _context.SaveChangesAsync();

                // Referee assignment for T2
                _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                {
                    RaceId = t2Race.RaceId,
                    RefereeId = refereeProfile.RefereeId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Active"
                });
                await _context.SaveChangesAsync();

                // Pending Bets for ongoing race
                _context.Bets.Add(new Bet
                {
                    UserId = spectatorUser.UserId,
                    RaceId = t2Race.RaceId,
                    HorseId = customHorses[0].HorseId,
                    Amount = 2000m,
                    Odds = t2Entries[0].CurrentOdds ?? 2.5m,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    RaceEntryId = t2Entries[0].RaceEntryId
                });
                _context.Bets.Add(new Bet
                {
                    UserId = spectator2User.UserId,
                    RaceId = t2Race.RaceId,
                    HorseId = customHorses[1].HorseId,
                    Amount = 1500m,
                    Odds = t2Entries[1].CurrentOdds ?? 2.9m,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    RaceEntryId = t2Entries[1].RaceEntryId
                });
                await _context.SaveChangesAsync();

                // Pending Predictions for ongoing race
                _context.Predictions.Add(new Prediction
                {
                    UserId = spectatorUser.UserId,
                    RaceId = t2Race.RaceId,
                    RaceEntryId = t2Entries[0].RaceEntryId,
                    PredictedAt = DateTime.UtcNow,
                    Status = "Pending",
                    IsCorrect = null,
                    Point = 0
                });
                _context.Predictions.Add(new Prediction
                {
                    UserId = spectator2User.UserId,
                    RaceId = t2Race.RaceId,
                    RaceEntryId = t2Entries[1].RaceEntryId,
                    PredictedAt = DateTime.UtcNow,
                    Status = "Pending",
                    IsCorrect = null,
                    Point = 0
                });
                await _context.SaveChangesAsync();

                // 4.1 Seed 15 extra registrations and pending jockey contracts (invitations) for Summer 2026
                var extraRegistrations = new List<Registration>();
                var nonCustomHorses = allHorses.Where(h => !customHorseNames.Contains(h.Name)).ToList();
                for (int i = 0; i < Math.Min(15, nonCustomHorses.Count); i++)
                {
                    var horse = nonCustomHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t2.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc)
                    };
                    extraRegistrations.Add(reg);
                }
                _context.Registrations.AddRange(extraRegistrations);
                await _context.SaveChangesAsync();

                var extraContracts = new List<JockeyContract>();
                for (int i = 0; i < extraRegistrations.Count; i++)
                {
                    var assignedJockey = allJockeys[i % allJockeys.Count];
                    var contract = new JockeyContract
                    {
                        TournamentId = t2.TournamentId,
                        HorseId = extraRegistrations[i].HorseId,
                        JockeyId = assignedJockey.UserId,
                        StartDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                        EndDate = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Accepted",
                        CreatedAt = DateTime.UtcNow
                    };
                    extraContracts.Add(contract);
                }
                _context.JockeyContracts.AddRange(extraContracts);
                await _context.SaveChangesAsync();
            }

            // 5. Seed Tournament 3: "Giải Đua Ngựa Mùa Đông 2026" (UPCOMING)
            var t3Name = "Winter Horse Racing Tournament 2026";
            var t3 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t3Name);
            if (t3 == null)
            {
                t3 = new Tournament
                {
                    Name = t3Name,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t3);
                await _context.SaveChangesAsync();

                var preRound = new Round
                {
                    TournamentId = t3.TournamentId,
                    Name = "Qualifier",
                    RoundNumber = 1,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Scheduled"
                };
                _context.Rounds.Add(preRound);
                await _context.SaveChangesAsync();

                var preRace1 = new Race
                {
                    RoundId = preRound.RoundId,
                    Name = "Pre Race 1",
                    RaceDate = new DateTime(2026, 12, 2, 9, 0, 0, DateTimeKind.Utc),
                    DistanceMeter = 1200,
                    MaxLanes = 12,
                    Status = "Scheduled"
                };
                _context.Races.Add(preRace1);
                await _context.SaveChangesAsync();

                // Approved registrations for T3 using our custom horses
                for (int i = 0; i < customHorses.Count; i++)
                {
                    var horse = customHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t3.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = DateTime.UtcNow
                    };
                    _context.Registrations.Add(reg);
                    await _context.SaveChangesAsync();

                    var jockey = allJockeys[i % allJockeys.Count];
                    var contract = new JockeyContract
                    {
                        TournamentId = t3.TournamentId,
                        HorseId = horse.HorseId,
                        JockeyId = jockey.UserId,
                        StartDate = t3.StartDate ?? DateTime.UtcNow,
                        EndDate = t3.EndDate ?? DateTime.UtcNow,
                        Status = "Accepted",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.JockeyContracts.Add(contract);
                    await _context.SaveChangesAsync();

                    _context.RaceEntries.Add(new RaceEntry
                    {
                        RaceId = preRace1.RaceId,
                        RegistrationId = reg.RegistrationId,
                        JockeyId = jockey.JockeyId,
                        LaneNo = i + 1,
                        Status = "Ready",
                        WinningProbability = 0m,
                        CurrentOdds = 2.0m
                    });
                }
                await _context.SaveChangesAsync();

                // Seed PENDING Registrations for T3 so Admin can test approving/rejecting
                var nonCustomHorsesForPending = allHorses.Where(h => !customHorseNames.Contains(h.Name)).ToList();
                for (int i = 0; i < Math.Min(5, nonCustomHorsesForPending.Count); i++)
                {
                    var horse = nonCustomHorsesForPending[i];
                    if (!await _context.Registrations.AnyAsync(r => r.TournamentId == t3.TournamentId && r.HorseId == horse.HorseId))
                    {
                        _context.Registrations.Add(new Registration
                        {
                            TournamentId = t3.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending",
                            RegisteredAt = DateTime.UtcNow.AddHours(-i)
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }


            // 6. Seed Tournament 5: "Giải mùa Xuân 2027" (UPCOMING)
            var t5Name = "Spring Tournament 2027";
            var t5 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t5Name);
            if (t5 == null)
            {
                t5 = new Tournament
                {
                    Name = t5Name,
                    StartDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t5);
                await _context.SaveChangesAsync();

                var preRound = new Round
                {
                    TournamentId = t5.TournamentId,
                    Name = "Qualifier",
                    RoundNumber = 1,
                    StartDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Scheduled"
                };
                _context.Rounds.Add(preRound);
                await _context.SaveChangesAsync();

                var finalRound = new Round
                {
                    TournamentId = t5.TournamentId,
                    Name = "Finals",
                    RoundNumber = 2,
                    StartDate = new DateTime(2027, 3, 6, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2027, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Scheduled"
                };
                _context.Rounds.Add(finalRound);
                await _context.SaveChangesAsync();

                var t5Registrations = new List<Registration>();
                for (int i = 0; i < Math.Min(15, allHorses.Count); i++)
                {
                    var horse = allHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t5.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = DateTime.UtcNow
                    };
                    t5Registrations.Add(reg);
                }
                _context.Registrations.AddRange(t5Registrations);
                await _context.SaveChangesAsync();

                var t5Contracts = new List<JockeyContract>();
                for (int i = 0; i < t5Registrations.Count; i++)
                {
                    var assignedJockey = allJockeys[i % allJockeys.Count];
                    var contract = new JockeyContract
                    {
                        TournamentId = t5.TournamentId,
                        HorseId = t5Registrations[i].HorseId,
                        JockeyId = assignedJockey.UserId,
                        StartDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                        EndDate = new DateTime(2027, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Accepted",
                        CreatedAt = DateTime.UtcNow
                    };
                    t5Contracts.Add(contract);
                }
                _context.JockeyContracts.AddRange(t5Contracts);
                await _context.SaveChangesAsync();
            }

            // 6.5 Seed Tournament 6: Expired Registration Tournament (Test Extend)
            var t6Name = "Expired Registration Tournament (Test Extend)";
            var t6 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t6Name);
            if (t6 == null)
            {
                t6 = new Tournament
                {
                    Name = t6Name,
                    Description = "Tournament seeded to test the extend registration feature when the deadline is past.",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(-1),
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    Status = "Registration Open",
                    CancelCount = 0
                };
                _context.Tournaments.Add(t6);
                await _context.SaveChangesAsync();
            }

            // Recalculate stats for custom horses to ensure perfect data consistency
            foreach (var name in customHorseNames)
            {
                var h = allHorses.First(x => x.Name == name);
                var entries = await _context.RaceEntries
                    .Include(re => re.Race)
                    .Where(re => re.Registration != null && re.Registration.HorseId == h.HorseId)
                    .Where(re => re.FinishTime.HasValue && re.FinishTime.Value > 0)
                    .Where(re => re.Race!.Status == "Completed" || re.Race.Status == "Finished")
                    .ToListAsync();
                
                if (entries.Any())
                {
                    var avg = entries.Average(re => re.FinishTime!.Value);
                    var recent = entries
                        .OrderByDescending(re => re.Race!.RaceDate)
                        .Take(3)
                        .Average(re => re.FinishTime!.Value);
                    var total = entries.Count;
                    var wins = entries.Count(re => re.FinishPosition == 1);
                    var winRate = (decimal)wins / total;

                    h.AverageTime = Math.Round(avg, 2);
                    h.RecentAverageTime = Math.Round(recent, 2);
                    h.WinRate = Math.Round(winRate, 2);
                }
            }
            await _context.SaveChangesAsync();

            // Recalculate and force update Odds for Summer and Winter races on startup
            var racesToRecalculate = new[] { "Summer Opening Race", "Pre Race 1" };
            foreach (var rName in racesToRecalculate)
            {
                var rObj = await _context.Races.Include(r => r.Round).FirstOrDefaultAsync(r => r.Name == rName);
                if (rObj != null)
                {
                    var entries = await _context.RaceEntries.Include(re => re.Registration).ThenInclude(reg => reg.Horse).Where(re => re.RaceId == rObj.RaceId).ToListAsync();
                    if (entries.Any())
                    {
                        var scores = new List<(RaceEntry entry, decimal score)>();
                        foreach (var entry in entries)
                        {
                            var horse = entry.Registration?.Horse;
                            var avg = horse?.AverageTime ?? 70m;
                            var rec = horse?.RecentAverageTime ?? avg;
                            var win = horse?.WinRate ?? 0.05m;
                            if (win > 1) win /= 100m;

                            var avgScore = Math.Max(1m, 100m - (avg - 60m) * 5m);
                            var recScore = Math.Max(1m, 100m - (rec - 60m) * 5m);
                            var winScore = win * 100m;

                            var score = avgScore * 0.4m + recScore * 0.4m + winScore * 0.2m;
                            scores.Add((entry, score));
                        }
                        var totScore = scores.Sum(x => x.score);
                        foreach (var item in scores)
                        {
                            var prob = item.score / totScore;
                            item.entry.WinningProbability = Math.Round(prob * 100m, 2);
                            item.entry.CurrentOdds = Math.Round(Math.Max((1m / prob) * 0.9m, 1.05m), 2);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // 7. Seed Demo Notifications
            if (!await _context.Notifications.AnyAsync())
            {
                var spectator = await _context.Users.FirstOrDefaultAsync(u => u.Username == "spectator");
                if (spectator != null)
                {
                    var notifications = new List<Notification>
                    {
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "Bet Placed Successfully",
                            Content = "You successfully placed a $100 bet on horse 'Warrior' in Race 5.",
                            Type = "Bet",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                            IsDeleted = false
                        },
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "Betting Open",
                            Content = "Tournament 'Spring Cup 2026' is now open for betting.",
                            Type = "Tournament",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                            IsDeleted = false
                        },
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "You Won the Bet!",
                            Content = "Congratulations! You won your bet on horse 'Glory' in Race 3. Received $250.",
                            Type = "Bet",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-1),
                            IsDeleted = false
                        },
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "Race 5 Started",
                            Content = "Race 5 has officially started. Watch the live results!",
                            Type = "Race",
                            IsRead = true,
                            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                            IsDeleted = false
                        },
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "Bet Refunded",
                            Content = "Race 2 was cancelled. Your $150 bet has been refunded to your wallet.",
                            Type = "Wallet",
                            IsRead = true,
                            CreatedAt = DateTime.UtcNow.AddHours(-3),
                            IsDeleted = false
                        },
                        new Notification
                        {
                            UserId = spectator.UserId,
                            Title = "System Maintenance",
                            Content = "The system will undergo scheduled maintenance from 02:00 to 04:00 tomorrow.",
                            Type = "System",
                            IsRead = true,
                            CreatedAt = DateTime.UtcNow.AddHours(-12),
                            IsDeleted = false
                        }
                    };
                    await _context.Notifications.AddRangeAsync(notifications);
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Rich demo data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding developer demo data.");
            throw;
        }
    }
}
