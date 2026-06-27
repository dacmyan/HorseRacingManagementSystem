using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Infrastructure.Persistence;

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
        _logger.LogInformation("Starting developer/test demo data seeding...");

        try
        {
            var hasher = new PasswordHasher<AppUser>();
            var fixedDate = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc);

            // 1. Seed Demo Users & Wallets & Profiles
            var demoUsers = new[]
            {
                new { Username = "owner",     Email = "owner@gmail.com",     FullName = "Nguyễn Văn Hùng", RoleId = 2 },
                new { Username = "owner2",    Email = "owner2@gmail.com",    FullName = "Trần Thị Mai",     RoleId = 2 },
                new { Username = "owner3",    Email = "owner3@gmail.com",    FullName = "Lê Minh Tuấn",     RoleId = 2 },
                new { Username = "jockey",    Email = "jockey@gmail.com",    FullName = "Jockey Nguyễn",    RoleId = 3 },
                new { Username = "referee",   Email = "referee@gmail.com",   FullName = "Trọng tài Nam",    RoleId = 4 },
                new { Username = "spectator", Email = "spectator@gmail.com", FullName = "Khán giả Bình",    RoleId = 5 }
            };

            foreach (var item in demoUsers)
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
                    await _context.SaveChangesAsync(); // save to generate ID

                    // Setup Profiles & Wallets
                    if (item.RoleId == 3) // Jockey
                    {
                        _context.JockeyProfiles.Add(new JockeyProfile
                        {
                            UserId = user.UserId,
                            ExperienceYears = 5,
                            RankingPoint = 150,
                            Status = "Active"
                        });
                    }
                    else if (item.RoleId == 4) // Referee
                    {
                        _context.RefereeProfiles.Add(new RefereeProfile
                        {
                            UserId = user.UserId,
                            LicenseNumber = "LIC-REF-DEMO",
                            ExperienceYears = 7,
                            Status = "Active"
                        });
                    }

                    // Create starting wallets for all users
                    var initialBalance = item.RoleId == 5 ? 10000.00m : 1000.00m;
                    _context.Wallets.Add(new Wallet
                    {
                        UserId = user.UserId,
                        Balance = initialBalance
                    });

                    await _context.SaveChangesAsync();
                }
            }

            // Seed 20 Referees dynamically for assignment tests
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
                        FullName = $"Trọng tài {i}",
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

            // Resolve entities needed for tournament seeding
            var ownerUser = await _context.Users.FirstAsync(u => u.Username == "owner");
            var owner2User = await _context.Users.FirstAsync(u => u.Username == "owner2");
            var owner3User = await _context.Users.FirstAsync(u => u.Username == "owner3");
            var jockeyUser = await _context.Users.FirstAsync(u => u.Username == "jockey");
            var refereeUser = await _context.Users.FirstAsync(u => u.Username == "referee");

            // 2. Seed Horses
            var baseDate = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc);
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
                new { Name = "War Admiral", Age = baseDate.AddYears(-6), Gender = "Stallion", Breed = "Thoroughbred", OwnerId = owner3User.UserId, AvgTime = 68.40m, RecentAvgTime = 68.40m, WinRate = 0.70m }
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

            // 3. Seed Tournament "Giải Đua Ngựa Mùa Đông 2026"
            var tournamentName = "Giải Đua Ngựa Mùa Đông 2026";
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == tournamentName);

            if (tournament == null)
            {
                tournament = new Tournament
                {
                    Name = tournamentName,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Upcoming"
                };
                _context.Tournaments.Add(tournament);
                await _context.SaveChangesAsync();

                // 4. Seed Tournament Rounds (Pre & Final)
                var preRound = new Round
                {
                    TournamentId = tournament.TournamentId,
                    Name = "Pre",
                    RoundNumber = 1,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Scheduled"
                };
                var finalRound = new Round
                {
                    TournamentId = tournament.TournamentId,
                    Name = "Final",
                    RoundNumber = 2,
                    StartDate = new DateTime(2026, 12, 6, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Scheduled"
                };
                _context.Rounds.AddRange(preRound, finalRound);
                await _context.SaveChangesAsync();

                // 5. Seed Races (Pre Race 1, Pre Race 2, Final Race)
                var preRace1 = new Race
                {
                    RoundId = preRound.RoundId,
                    Name = "Pre Race 1",
                    RaceDate = new DateTime(2026, 12, 2, 9, 0, 0, DateTimeKind.Utc),
                    DistanceMeter = 1200,
                    MaxLanes = 12,
                    Status = "Scheduled"
                };
                var preRace2 = new Race
                {
                    RoundId = preRound.RoundId,
                    Name = "Pre Race 2",
                    RaceDate = new DateTime(2026, 12, 3, 9, 0, 0, DateTimeKind.Utc),
                    DistanceMeter = 1200,
                    MaxLanes = 12,
                    Status = "Scheduled"
                };
                var finalRace = new Race
                {
                    RoundId = finalRound.RoundId,
                    Name = "Chung Kết Mùa Đông",
                    RaceDate = new DateTime(2026, 12, 9, 15, 0, 0, DateTimeKind.Utc),
                    DistanceMeter = 1600,
                    MaxLanes = 12,
                    Status = "Scheduled"
                };
                _context.Races.AddRange(preRace1, preRace2, finalRace);
                await _context.SaveChangesAsync();

                // 6. Seed Registrations for tournament (Register all 14 horses)
                var horses = await _context.Horses.ToListAsync();
                var registrations = new List<Registration>();

                foreach (var horse in horses)
                {
                    var registration = new Registration
                    {
                        TournamentId = tournament.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = DateTime.UtcNow
                    };
                    registrations.Add(registration);
                }
                _context.Registrations.AddRange(registrations);
                await _context.SaveChangesAsync();

                // 7. Seed Jockey Contracts (Approved contracts between registered horses and jockey)
                var jockeyContracts = new List<JockeyContract>();
                foreach (var reg in registrations)
                {
                    var contract = new JockeyContract
                    {
                        TournamentId = tournament.TournamentId,
                        HorseId = reg.HorseId,
                        JockeyId = jockeyUser.UserId,
                        StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                        EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Approved",
                        CreatedAt = DateTime.UtcNow
                    };
                    jockeyContracts.Add(contract);
                }
                _context.JockeyContracts.AddRange(jockeyContracts);
                await _context.SaveChangesAsync();

                // 8. Seed Race Entries for Pre Race 1 (Lanes 1 to 7) & Pre Race 2 (Lanes 1 to 7)
                var race1Entries = new List<RaceEntry>();
                var race2Entries = new List<RaceEntry>();

                for (int i = 0; i < registrations.Count; i++)
                {
                    var reg = registrations[i];
                    if (i < 7)
                    {
                        race1Entries.Add(new RaceEntry
                        {
                            RaceId = preRace1.RaceId,
                            RegistrationId = reg.RegistrationId,
                            JockeyId = jockeyUser.UserId,
                            LaneNo = i + 1,
                            Status = "Ready",
                            WinningProbability = 14.28m,
                            CurrentOdds = 2.0m
                        });
                    }
                    else
                    {
                        race2Entries.Add(new RaceEntry
                        {
                            RaceId = preRace2.RaceId,
                            RegistrationId = reg.RegistrationId,
                            JockeyId = jockeyUser.UserId,
                            LaneNo = (i - 7) + 1,
                            Status = "Ready",
                            WinningProbability = 14.28m,
                            CurrentOdds = 2.0m
                        });
                    }
                }
                _context.RaceEntries.AddRange(race1Entries);
                _context.RaceEntries.AddRange(race2Entries);
                await _context.SaveChangesAsync();

                // 9. Referee Assignments to races
                _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                {
                    RaceId = preRace1.RaceId,
                    RefereeId = refereeUser.UserId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Active"
                });
                _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                {
                    RaceId = preRace2.RaceId,
                    RefereeId = refereeUser.UserId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Active"
                });
                _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                {
                    RaceId = finalRace.RaceId,
                    RefereeId = refereeUser.UserId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Active"
                });
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeded Giải Đua Ngựa Mùa Đông 2026 with rounds, races, registrations, entries, and referee assignments.");
            }

            _logger.LogInformation("Demo data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding developer demo data.");
            throw;
        }
    }
}
