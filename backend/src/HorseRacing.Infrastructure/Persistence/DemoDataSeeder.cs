using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

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
        _logger.LogInformation("Starting developer/test demo data seeding...");

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
                new() { Username = "spectator", Email = "spectator@gmail.com", FullName = "Khán giả Bình",    RoleId = 5 }
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
                    await _context.SaveChangesAsync(); // save to generate ID

                    // Setup Profiles & Wallets
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
            var jockeys = await _context.JockeyProfiles.Include(jp => jp.User).ToListAsync();
            var refereeProfile = await _context.RefereeProfiles.Include(rp => rp.User).FirstAsync(rp => rp.User.Username == "referee");

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding developer demo data.");
            throw;
        }
    }
}
