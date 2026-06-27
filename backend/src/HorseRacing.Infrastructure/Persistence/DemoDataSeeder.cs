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
                new() { Username = "owner",     Email = "owner@gmail.com",     FullName = "Nguyễn Văn Hùng", RoleId = 2 },
                new() { Username = "owner2",    Email = "owner2@gmail.com",    FullName = "Trần Thị Mai",     RoleId = 2 },
                new() { Username = "owner3",    Email = "owner3@gmail.com",    FullName = "Lê Minh Tuấn",     RoleId = 2 },
                new() { Username = "jockey",    Email = "jockey@gmail.com",    FullName = "Jockey Nguyễn",    RoleId = 3 },
                new() { Username = "referee",   Email = "referee@gmail.com",   FullName = "Trọng tài Nam",    RoleId = 4 },
                new() { Username = "spectator", Email = "spectator@gmail.com", FullName = "Khán giả Bình",    RoleId = 5 },
                new() { Username = "spectator2", Email = "spectator2@gmail.com", FullName = "Khán giả Hoàng",   RoleId = 5 },
                new() { Username = "spectator3", Email = "spectator3@gmail.com", FullName = "Khán giả Dung",    RoleId = 5 }
            };

            for (int i = 1; i <= 10; i++)
            {
                demoUsersList.Add(new DemoUserSeedDto { Username = $"jockey{i}", Email = $"jockey{i}@gmail.com", FullName = $"Jockey Số {i}", RoleId = 3 });
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

            // 2. Seed 30 Horses
            var horseNames = new[]
            {
                "Red Rum", "Secretariat", "Seattle Slew", "Spectacular Bid", "Zenyatta",
                "Shergar", "Eclipse", "Affirmed", "Ruffian", "Frankel",
                "Man o' War", "Phar Lap", "Citation", "War Admiral", "Black Caviar",
                "Winx", "Kauto Star", "Desert Orchid", "Arkle", "Pegasus",
                "Silver Charm", "Thunder Gulch", "Smarty Jones", "Barbaro", "American Pharoah",
                "Justify", "Cigar", "Easy Goer", "Sunday Silence", "Deep Impact"
            };

            var random = new Random();
            for (int i = 0; i < horseNames.Length; i++)
            {
                int horseOwnerId = (i % 3) switch
                {
                    0 => ownerUser.UserId,
                    1 => owner2User.UserId,
                    _ => owner3User.UserId
                };

                _context.Horses.Add(new Horse
                {
                    Name = horseNames[i],
                    Age = fixedDate.AddYears(-3 - (i % 5)),
                    Gender = (i % 2 == 0) ? "Stallion" : "Mare",
                    Breed = "Thoroughbred",
                    HealthStatus = "Healthy",
                    OwnerId = horseOwnerId,
                    AverageTime = Math.Round(65m + (decimal)random.NextDouble() * 8m, 2),
                    RecentAverageTime = Math.Round(65m + (decimal)random.NextDouble() * 8m, 2),
                    WinRate = Math.Round((decimal)random.NextDouble() * 0.9m, 2)
                });
            }
            await _context.SaveChangesAsync();
            var allHorses = await _context.Horses.ToListAsync();

            // 3. Seed Tournament "Giải Đua Ngựa Mùa Đông 2026"
            var tournamentName = "Giải Đua Ngựa Mùa Đông 2026";
            var tournament = new Tournament
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

            // 5. Seed Races (Pre Race 1, Pre Race 2, Pre Race 3, Chung Kết Mùa Đông)
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
            var preRace3 = new Race
            {
                RoundId = preRound.RoundId,
                Name = "Pre Race 3",
                RaceDate = new DateTime(2026, 12, 4, 9, 0, 0, DateTimeKind.Utc),
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
            _context.Races.AddRange(preRace1, preRace2, preRace3, finalRace);
            await _context.SaveChangesAsync();

            // 6. Seed Registrations for tournament (Register all 30 horses)
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
            // 3. Seed Tournament 1: "Giải Đua Ngựa Mùa Xuân 2026" (FINISHED)
            var t1Name = "Giải Đua Ngựa Mùa Xuân 2026";
            var t1 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t1Name);
            if (t1 == null)
            {
                t1 = new Tournament
                {
                    Name = t1Name,
                    StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Completed"
                };
                _context.Tournaments.Add(t1);
                await _context.SaveChangesAsync();

                var t1Round = new Round
                {
                    TournamentId = t1.TournamentId,
                    Name = "Chung Kết",
                    RoundNumber = 1,
                    StartDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Completed"
                };
                _context.Rounds.Add(t1Round);
                await _context.SaveChangesAsync();

                var t1Race = new Race
                {
                    RoundId = t1Round.RoundId,
                    Name = "Trận Đại Chiến Mùa Xuân",
                    RaceDate = new DateTime(2026, 3, 8, 14, 0, 0, DateTimeKind.Utc),
                    DistanceMeter = 1600,
                    MaxLanes = 8,
                    Status = "Completed"
                };
                _context.Races.Add(t1Race);
                await _context.SaveChangesAsync();

                // Registrations & Entries for T1
                var t1Entries = new List<RaceEntry>();
                for (int i = 0; i < 6; i++)
                {
                    var horse = allHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t1.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc)
                    };
                    _context.Registrations.Add(reg);
                    await _context.SaveChangesAsync();

                    var entry = new RaceEntry
                    {
                        RaceId = t1Race.RaceId,
                        RegistrationId = reg.RegistrationId,
                        JockeyId = jockeyProf.JockeyId,
                        LaneNo = i + 1,
                        Status = "Finished",
                        WinningProbability = 20.0m,
                        CurrentOdds = 2.5m,
                        FinishTime = i == 1 ? 65.20m : (66.00m + i),
                        FinishPosition = i == 1 ? 1 : (i == 0 ? 2 : i + 1)
                    };
                    t1Entries.Add(entry);
                }
                _context.RaceEntries.AddRange(t1Entries);
                await _context.SaveChangesAsync();

                // Published RaceResult
                var secretariatHorse = allHorses.First(h => h.Name == "Secretariat");
                _context.RaceResults.Add(new RaceResult
                {
                    RaceId = t1Race.RaceId,
                    Winner = secretariatHorse.Name,
                    ResultRecordedAt = new DateTime(2026, 3, 8, 15, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2026, 3, 8, 15, 0, 0, DateTimeKind.Utc)
                });
                await _context.SaveChangesAsync();

                // Seed Bets on T1 Race
                var bet1 = new Bet
                {
                    UserId = spectatorUser.UserId,
                    RaceId = t1Race.RaceId,
                    HorseId = secretariatHorse.HorseId,
                    Amount = 5000m,
                    Odds = 2.5m,
                    Status = "Won",
                    CreatedAt = new DateTime(2026, 3, 8, 12, 0, 0, DateTimeKind.Utc),
                    RaceEntryId = t1Entries.First(e => e.FinishPosition == 1).RaceEntryId
                };
                var bet2 = new Bet
                {
                    UserId = spectator2User.UserId,
                    RaceId = t1Race.RaceId,
                    HorseId = allHorses[0].HorseId,
                    Amount = 3000m,
                    Odds = 3.0m,
                    Status = "Lost",
                    CreatedAt = new DateTime(2026, 3, 8, 12, 30, 0, DateTimeKind.Utc),
                    RaceEntryId = t1Entries.First(e => e.LaneNo == 1).RaceEntryId
                };
                var bet3 = new Bet
                {
                    UserId = spectator3User.UserId,
                    RaceId = t1Race.RaceId,
                    HorseId = secretariatHorse.HorseId,
                    Amount = 10000m,
                    Odds = 2.2m,
                    Status = "Won",
                    CreatedAt = new DateTime(2026, 3, 8, 13, 0, 0, DateTimeKind.Utc),
                    RaceEntryId = t1Entries.First(e => e.FinishPosition == 1).RaceEntryId
                };
                _context.Bets.AddRange(bet1, bet2, bet3);
                await _context.SaveChangesAsync();

                // Seed Payouts for Won Bets
                _context.Payouts.Add(new Payout
                {
                    BetId = bet1.Id,
                    Amount = 5000m * 2.5m,
                    CreatedAt = new DateTime(2026, 3, 8, 15, 5, 0, DateTimeKind.Utc)
                });
                _context.Payouts.Add(new Payout
                {
                    BetId = bet3.Id,
                    Amount = 10000m * 2.2m,
                    CreatedAt = new DateTime(2026, 3, 8, 15, 5, 0, DateTimeKind.Utc)
                });
                await _context.SaveChangesAsync();

                // Seed Predictions for T1
                _context.Predictions.Add(new Prediction
                {
                    UserId = spectatorUser.UserId,
                    RaceId = t1Race.RaceId,
                    RaceEntryId = t1Entries.First(e => e.FinishPosition == 1).RaceEntryId,
                    PredictedAt = new DateTime(2026, 3, 8, 10, 0, 0, DateTimeKind.Utc),
                    Status = "Evaluated",
                    IsCorrect = true,
                    Point = 100
                });
                _context.Predictions.Add(new Prediction
                {
                    UserId = spectator2User.UserId,
                    RaceId = t1Race.RaceId,
                    RaceEntryId = t1Entries.First(e => e.LaneNo == 1).RaceEntryId,
                    PredictedAt = new DateTime(2026, 3, 8, 10, 30, 0, DateTimeKind.Utc),
                    Status = "Evaluated",
                    IsCorrect = false,
                    Point = 0
                });
                _context.Predictions.Add(new Prediction
                {
                    UserId = spectator3User.UserId,
                    RaceId = t1Race.RaceId,
                    RaceEntryId = t1Entries.First(e => e.FinishPosition == 1).RaceEntryId,
                    PredictedAt = new DateTime(2026, 3, 8, 11, 0, 0, DateTimeKind.Utc),
                    Status = "Evaluated",
                    IsCorrect = true,
                    Point = 100
                });
                await _context.SaveChangesAsync();
            }

            // 4. Seed Tournament 2: "Giải Đua Ngựa Mùa Hè 2026" (ONGOING / LIVE)
            var t2Name = "Giải Đua Ngựa Mùa Hè 2026";
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
                    Name = "Vòng Loại 1",
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
                    Name = "Trận Đua Khai Mạc Mùa Hè",
                    RaceDate = DateTime.UtcNow.AddDays(1),
                    DistanceMeter = 1400,
                    MaxLanes = 8,
                    Status = "Ongoing"
                };
                _context.Races.Add(t2Race);
                await _context.SaveChangesAsync();

                var t2Entries = new List<RaceEntry>();
                for (int i = 0; i < 6; i++)
                {
                    var horse = allHorses[i + 4];
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
                        JockeyId = jockeyProf.JockeyId,
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
                    var horse = allHorses.First(h => h.HorseId == _context.Registrations.First(r => r.RegistrationId == entry.RegistrationId).HorseId);
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
                    RefereeId = refereeUser.UserId,
                    AssignedAt = DateTime.UtcNow,
                    Status = "Active"
                });
                await _context.SaveChangesAsync();

                // Pending Bets for ongoing race
                _context.Bets.Add(new Bet
                {
                    UserId = spectatorUser.UserId,
                    RaceId = t2Race.RaceId,
                    HorseId = allHorses[4].HorseId,
                    Amount = 2000m,
                    Odds = 2.5m,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    RaceEntryId = t2Entries[0].RaceEntryId
                });
                _context.Bets.Add(new Bet
                {
                    UserId = spectator2User.UserId,
                    RaceId = t2Race.RaceId,
                    HorseId = allHorses[5].HorseId,
                    Amount = 1500m,
                    Odds = 2.9m,
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
            }

            // 5. Seed Tournament 3: "Giải Đua Ngựa Mùa Đông 2026" (UPCOMING)
            var t3Name = "Giải Đua Ngựa Mùa Đông 2026";
            var t3 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t3Name);
            if (t3 == null)
            {
                t3 = new Tournament
                {
                    Name = t3Name,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Upcoming"
                };
                _context.Tournaments.Add(t3);
                await _context.SaveChangesAsync();

                var preRound = new Round
                {
                    TournamentId = t3.TournamentId,
                    Name = "Vòng Loại",
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

                // Approved registrations for T3
                for (int i = 0; i < 6; i++)
                {
                    var horse = allHorses[i];
                    var reg = new Registration
                    {
                        TournamentId = t3.TournamentId,
                        HorseId = horse.HorseId,
                        Status = "Approved",
                        RegisteredAt = DateTime.UtcNow
                    };
                    _context.Registrations.Add(reg);
                    await _context.SaveChangesAsync();

                    _context.RaceEntries.Add(new RaceEntry
                    {
                        RaceId = preRace1.RaceId,
                        RegistrationId = reg.RegistrationId,
                        JockeyId = jockeyProf.JockeyId,
                        LaneNo = i + 1,
                        Status = "Ready",
                        WinningProbability = 16.6m,
                        CurrentOdds = 2.0m
                    });
                }
                await _context.SaveChangesAsync();

                // 6. Seed PENDING Registrations for T3 so Admin can test approving/rejecting in /admin/registrations
                for (int i = 6; i < 11; i++)
                {
                    var horse = allHorses[i];
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
            _context.Registrations.AddRange(registrations);
            await _context.SaveChangesAsync();

            // 7. Seed Jockey Contracts (Distribute 11 jockeys dynamically to 30 horses)
            var jockeyContracts = new List<JockeyContract>();
            for (int i = 0; i < registrations.Count; i++)
            {
                var reg = registrations[i];
                var assignedJockey = jockeys[i % jockeys.Count];

                var contract = new JockeyContract
                {
                    TournamentId = tournament.TournamentId,
                    HorseId = reg.HorseId,
                    JockeyId = assignedJockey.UserId,
                    StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Approved",
                    CreatedAt = DateTime.UtcNow
                };
                jockeyContracts.Add(contract);
            }
            _context.JockeyContracts.AddRange(jockeyContracts);
            await _context.SaveChangesAsync();

            // 8. Seed Race Entries (Pre Race 1: 12 lanes, Pre Race 2: 12 lanes, Pre Race 3: 6 lanes)
            var raceEntries = new List<RaceEntry>();
            for (int i = 0; i < registrations.Count; i++)
            {
                var reg = registrations[i];
                var assignedJockey = jockeys[i % jockeys.Count];

                long targetRaceId;
                int laneNo;

                if (i < 12)
                {
                    targetRaceId = preRace1.RaceId;
                    laneNo = i + 1;
                }
                else if (i < 24)
                {
                    targetRaceId = preRace2.RaceId;
                    laneNo = (i - 12) + 1;
                }
                else
                {
                    targetRaceId = preRace3.RaceId;
                    laneNo = (i - 24) + 1;
                }

                raceEntries.Add(new RaceEntry
                {
                    RaceId = targetRaceId,
                    RegistrationId = reg.RegistrationId,
                    JockeyId = assignedJockey.JockeyId,
                    LaneNo = laneNo,
                    Status = "Ready",
                    WinningProbability = 8.33m,
                    CurrentOdds = 2.0m
                });
            }
            _context.RaceEntries.AddRange(raceEntries);
            await _context.SaveChangesAsync();

            // 9. Referee Assignments to races
            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
            {
                RaceId = preRace1.RaceId,
                RefereeId = refereeProfile.RefereeId,
                AssignedAt = DateTime.UtcNow,
                Status = "Active"
            });
            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
            {
                RaceId = preRace2.RaceId,
                RefereeId = refereeProfile.RefereeId,
                AssignedAt = DateTime.UtcNow,
                Status = "Active"
            });
            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
            {
                RaceId = preRace3.RaceId,
                RefereeId = refereeProfile.RefereeId,
                AssignedAt = DateTime.UtcNow,
                Status = "Active"
            });
            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
            {
                RaceId = finalRace.RaceId,
                RefereeId = refereeProfile.RefereeId,
                AssignedAt = DateTime.UtcNow,
                Status = "Active"
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded Giải Đua Ngựa Mùa Đông 2026 with 30 horses, 11 jockeys, and 3 pre races.");
            _logger.LogInformation("Demo data seeding completed successfully.");
            // Always recalculate and force update Odds for Summer Race on every startup
            var summerRace = await _context.Races.Include(r => r.Round).FirstOrDefaultAsync(r => r.Name == "Trận Đua Khai Mạc Mùa Hè");
            if (summerRace != null)
            {
                var summerEntries = await _context.RaceEntries.Include(re => re.Registration).ThenInclude(reg => reg.Horse).Where(re => re.RaceId == summerRace.RaceId).ToListAsync();
                if (summerEntries.Any())
                {
                    var scores = new List<(RaceEntry entry, decimal score)>();
                    foreach (var entry in summerEntries)
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

            _logger.LogInformation("Rich demo data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding developer demo data.");
            throw;
        }
    }
}
