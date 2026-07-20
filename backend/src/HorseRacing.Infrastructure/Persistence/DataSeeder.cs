using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Infrastructure.Persistence;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting mandatory data seeding...");

        try
        {
            // 1. Seed Roles
            var roles = new[]
            {
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "HorseOwner" },
                new Role { RoleId = 3, Name = "Jockey" },
                new Role { RoleId = 4, Name = "Referee" },
                new Role { RoleId = 5, Name = "Spectator" },
                new Role { RoleId = 6, Name = "Veterinarian" }
            };

            bool roleAdded = false;
            foreach (var role in roles)
            {
                if (!await _context.Roles.AnyAsync(r => r.RoleId == role.RoleId))
                {
                    _context.Roles.Add(role);
                    roleAdded = true;
                }
            }

            if (roleAdded)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("System roles seeded successfully.");
            }

            // 2. Seed Default Admin User
            var hasher = new PasswordHasher<AppUser>();
            var defaultAdminEmail = "admin@gmail.com";
            var defaultAdminUsername = "admin";

            if (!await _context.Users.AnyAsync(u => u.Email == defaultAdminEmail || u.Username == defaultAdminUsername))
            {
                var adminUser = new AppUser
                {
                    Username = defaultAdminUsername,
                    Email = defaultAdminEmail,
                    FullName = "System Administrator",
                    RoleId = 1, // Admin Role
                    Status = "Active",
                    IsEmailConfirmed = true, // System account — no email verification needed
                    CreatedAt = DateTime.UtcNow
                };
                adminUser.PasswordHash = hasher.HashPassword(adminUser, "123456");

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Default Admin user ('admin@gmail.com' / '123456') seeded successfully.");
            }

            // 3. Seed Default Veterinarian User
            var defaultVetEmail = "vet@gmail.com";
            var defaultVetUsername = "vet";

            if (!await _context.Users.AnyAsync(u => u.Email == defaultVetEmail || u.Username == defaultVetUsername))
            {
                var vetUser = new AppUser
                {
                    Username = defaultVetUsername,
                    Email = defaultVetEmail,
                    FullName = "System Veterinarian",
                    RoleId = 6, // Veterinarian Role
                    Status = "Active",
                    IsEmailConfirmed = true, // System account — no email verification needed
                    CreatedAt = DateTime.UtcNow
                };
                vetUser.PasswordHash = hasher.HashPassword(vetUser, "123456");

                _context.Users.Add(vetUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Default Veterinarian user ('vet@gmail.com' / '123456') seeded successfully.");
            }

            // 3b. Seed Default Referee User
            var defaultRefereeEmail = "referee@gmail.com";
            var defaultRefereeUsername = "referee";

            if (!await _context.Users.AnyAsync(u => u.Email == defaultRefereeEmail || u.Username == defaultRefereeUsername))
            {
                var refereeUser = new AppUser
                {
                    Username = defaultRefereeUsername,
                    Email = defaultRefereeEmail,
                    FullName = "System Referee",
                    RoleId = 4, // Referee Role
                    Status = "Active",
                    IsEmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                refereeUser.PasswordHash = hasher.HashPassword(refereeUser, "123456");

                _context.Users.Add(refereeUser);
                await _context.SaveChangesAsync();

                _context.RefereeProfiles.Add(new RefereeProfile
                {
                    UserId = refereeUser.UserId,
                    LicenseNumber = "REF-12345",
                    Status = "Active"
                });
                await _context.SaveChangesAsync();
                _logger.LogInformation("Default Referee user ('referee@gmail.com' / '123456') seeded successfully.");
            }

            // 4. Seed 25 Test Jockey Users
            for (int i = 1; i <= 25; i++)
            {
                var jockeyUsername = $"jockeytest{i}";
                var jockeyEmail = $"jockeytest{i}@gmail.com";
                if (!await _context.Users.AnyAsync(u => u.Username == jockeyUsername || u.Email == jockeyEmail))
                {
                    var user = new AppUser
                    {
                        Username = jockeyUsername,
                        Email = jockeyEmail,
                        FullName = $"Jockey-Test{i}",
                        RoleId = 3, // Jockey
                        Status = "Active",
                        IsEmailConfirmed = true, // Seeded test account — no email verification needed
                        CreatedAt = DateTime.UtcNow
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
                    _logger.LogInformation($"Test Jockey user ('{jockeyEmail}' / '123456') seeded successfully.");
                }
            }

            // 5. Seed Owner-3 and 12 Horses
            var owner3Username = "owner3";
            var owner3Email = "owner3@gmail.com";
            AppUser? owner3User = await _context.Users.FirstOrDefaultAsync(u => u.Username == owner3Username || u.Email == owner3Email);
            if (owner3User == null)
            {
                owner3User = new AppUser
                {
                    Username = owner3Username,
                    Email = owner3Email,
                    FullName = "Owner-3 (David Le)",
                    RoleId = 2, // HorseOwner Role
                    Status = "Active",
                    IsEmailConfirmed = true, // Seeded test account — no email verification needed
                    CreatedAt = DateTime.UtcNow
                };
                owner3User.PasswordHash = hasher.HashPassword(owner3User, "123456");
                _context.Users.Add(owner3User);
                await _context.SaveChangesAsync();

                _context.Wallets.Add(new Wallet
                {
                    UserId = owner3User.UserId,
                    Balance = 10000m
                });
                await _context.SaveChangesAsync();
                _logger.LogInformation("Test Owner-3 user seeded successfully.");
            }

            for (int i = 1; i <= 25; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                if (!await _context.Horses.AnyAsync(h => h.Name == horseName))
                {
                    decimal avgSpeed = 14.50m;
                    decimal recentSpeed = 14.50m;
                    decimal winRate = 0.20m;

                    if (i == 1) // Very strong horse
                    {
                        avgSpeed = 17.50m;
                        recentSpeed = 18.00m;
                        winRate = 0.80m;
                    }
                    else if (i == 2) // Very slow horse
                    {
                        avgSpeed = 11.50m;
                        recentSpeed = 11.00m;
                        winRate = 0.05m;
                    }
                    else if (i == 3) // Above average
                    {
                        avgSpeed = 15.80m;
                        recentSpeed = 16.00m;
                        winRate = 0.40m;
                    }

                    var horse = new Horse
                    {
                        Name = horseName,
                        Age = DateTime.UtcNow.AddYears(-5),
                        Gender = i % 2 == 0 ? "Stallion" : "Mare",
                        Breed = "Thoroughbred",
                        HealthStatus = "Healthy",
                        OwnerId = owner3User.UserId,
                        AverageTime = avgSpeed,
                        RecentAverageTime = recentSpeed,
                        WinRate = winRate
                    };
                    _context.Horses.Add(horse);
                    await _context.SaveChangesAsync();

                    _context.HorseStatistics.Add(new HorseStatistic
                    {
                        HorseId = horse.HorseId,
                        TotalRaces = 0,
                        TotalWins = 0,
                        TotalSecondPlaces = 0,
                        TotalThirdPlaces = 0,
                        AverageSpeed = 0m,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Test Horse '{horseName}' (Speed: {avgSpeed} m/s) seeded successfully.");
                }
            }

            // 6. Seed Tournament "Giải Test 5" and register Owner-3's 12 horses to it and invite the 12 Jockey-Test Jockeys
            var t5Name = "Test Tournament 5";
            var t5 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t5Name);
            if (t5 == null)
            {
                t5 = new Tournament
                {
                    Name = t5Name,
                    Description = "Test Tournament 5 Description",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                    StartDate = DateTime.UtcNow.AddDays(6),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t5);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{t5Name}' seeded successfully.");
            }

            for (int i = 1; i <= 12; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // 6a. Seed JockeyContract invitation
                    var hasContract = await _context.JockeyContracts.AnyAsync(jc => jc.TournamentId == t5.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (!hasContract)
                    {
                        var contract = new JockeyContract
                        {
                            TournamentId = t5.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = t5.StartDate ?? DateTime.UtcNow.AddDays(6),
                            EndDate = t5.EndDate ?? DateTime.UtcNow.AddDays(15),
                            Status = "Pending",
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{t5Name}' seeded successfully.");
                    }

                    // 6b. Seed Tournament Registration
                    var hasRegistration = await _context.Registrations.AnyAsync(r => r.TournamentId == t5.TournamentId && r.HorseId == horse.HorseId);
                    if (!hasRegistration)
                    {
                        var registration = new Registration
                        {
                            TournamentId = t5.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending",
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{t5Name}' seeded successfully.");
                    }
                }
            }

            // 6.1. Seed Tournament "Expired Registration Tournament (Test Extend)"
            var tExtendTestName = "Expired Registration Tournament (Test Extend)";
            var tExtendTest = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == tExtendTestName);
            if (tExtendTest == null)
            {
                tExtendTest = new Tournament
                {
                    Name = tExtendTestName,
                    Description = "Seeded tournament to test extend registration feature.",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(-1),
                    StartDate = DateTime.UtcNow.AddDays(5),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    Status = "Registration Open",
                    CancelCount = 0
                };
                _context.Tournaments.Add(tExtendTest);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{tExtendTestName}' seeded successfully.");
            }
            else
            {
                tExtendTest.RegistrationEndDate = DateTime.UtcNow.AddDays(-1);
                tExtendTest.StartDate = DateTime.UtcNow.AddDays(5);
                tExtendTest.EndDate = DateTime.UtcNow.AddDays(15);
                tExtendTest.CancelCount = 0;
                _context.Tournaments.Update(tExtendTest);
                await _context.SaveChangesAsync();
            }

            // 6.2. Seed Tournament "Giải Test 1" and register Owner-3's 12 horses to it and invite the 12 Jockey-Test Jockeys
            var t1Name = "Test Tournament 1";
            var t1 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t1Name);
            if (t1 == null)
            {
                t1 = new Tournament
                {
                    Name = t1Name,
                    Description = "Test Tournament 1 Description",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(-1),
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t1);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{t1Name}' seeded successfully.");
            }
            else
            {
                t1.RegistrationEndDate = DateTime.UtcNow.AddDays(-1);
                t1.StartDate = DateTime.UtcNow.AddDays(1);
                t1.EndDate = DateTime.UtcNow.AddDays(10);
                _context.Tournaments.Update(t1);
                await _context.SaveChangesAsync();
            }

            for (int i = 1; i <= 12; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // 6.2a. Seed JockeyContract invitation
                    var hasContract = await _context.JockeyContracts.AnyAsync(jc => jc.TournamentId == t1.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (!hasContract)
                    {
                        var contract = new JockeyContract
                        {
                            TournamentId = t1.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = t1.StartDate ?? DateTime.UtcNow.AddDays(6),
                            EndDate = t1.EndDate ?? DateTime.UtcNow.AddDays(15),
                            Status = "Pending",
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{t1Name}' seeded successfully.");
                    }

                    // 6.2b. Seed Tournament Registration
                    var hasRegistration = await _context.Registrations.AnyAsync(r => r.TournamentId == t1.TournamentId && r.HorseId == horse.HorseId);
                    if (!hasRegistration)
                    {
                        var registration = new Registration
                        {
                            TournamentId = t1.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending",
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{t1Name}' seeded successfully.");
                    }
                }
            }

            // 6.5. Seed Tournament "Giải Test 6" and register Owner-3's 12 horses to it and invite the 12 Jockey-Test Jockeys (Pending)
            var t6Name = "Test Tournament 6";
            var t6 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t6Name);
            if (t6 == null)
            {
                t6 = new Tournament
                {
                    Name = t6Name,
                    Description = "Test Tournament 6 Description",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                    StartDate = DateTime.UtcNow.AddDays(6),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t6);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{t6Name}' seeded successfully.");
            }

            for (int i = 1; i <= 25; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // 6.5a. Seed JockeyContract invitation (Pending status)
                    var hasContract = await _context.JockeyContracts.AnyAsync(jc => jc.TournamentId == t6.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (!hasContract)
                    {
                        var contract = new JockeyContract
                        {
                            TournamentId = t6.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = t6.StartDate ?? DateTime.UtcNow.AddDays(6),
                            EndDate = t6.EndDate ?? DateTime.UtcNow.AddDays(15),
                            Status = "Pending",
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{t6Name}' seeded successfully.");
                    }

                    // 6.5b. Seed Tournament Registration
                    var hasRegistration = await _context.Registrations.AnyAsync(r => r.TournamentId == t6.TournamentId && r.HorseId == horse.HorseId);
                    if (!hasRegistration)
                    {
                        var registration = new Registration
                        {
                            TournamentId = t6.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending",
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{t6Name}' seeded successfully.");
                    }
                }
            }

            // 6.6. Seed Tournament "Giải Test 7" and register Owner-3's 12 horses to it and invite the 12 Jockey-Test Jockeys (Pending)
            var t7Name = "Test Tournament 7";
            var t7 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t7Name);
            if (t7 == null)
            {
                t7 = new Tournament
                {
                    Name = t7Name,
                    Description = "Test Tournament 7 Description",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                    StartDate = DateTime.UtcNow.AddDays(6),
                    EndDate = DateTime.UtcNow.AddDays(15),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(t7);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{t7Name}' seeded successfully.");
            }

            for (int i = 1; i <= 12; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // 6.6a. Seed JockeyContract invitation
                    var hasContract = await _context.JockeyContracts.AnyAsync(jc => jc.TournamentId == t7.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (!hasContract)
                    {
                        var contract = new JockeyContract
                        {
                            TournamentId = t7.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = t7.StartDate ?? DateTime.UtcNow.AddDays(6),
                            EndDate = t7.EndDate ?? DateTime.UtcNow.AddDays(15),
                            Status = "Pending",
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{t7Name}' seeded successfully.");
                    }

                    // 6.6b. Seed Tournament Registration
                    var hasRegistration = await _context.Registrations.AnyAsync(r => r.TournamentId == t7.TournamentId && r.HorseId == horse.HorseId);
                    if (!hasRegistration)
                    {
                        var registration = new Registration
                        {
                            TournamentId = t7.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending",
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{t7Name}' seeded successfully.");
                    }
                }
            }

            // 6.6c. Seed Tournament "Dac Tour 3"
            var dacTour3Name = "Dac Tour 3";
            var dacTour3 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == dacTour3Name);
            if (dacTour3 == null)
            {
                dacTour3 = new Tournament
                {
                    Name = dacTour3Name,
                    Description = "Dac Tour 3 Tournament with accepted jockey contracts",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-10),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(dacTour3);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{dacTour3Name}' seeded successfully.");
            }

            for (int i = 1; i <= 12; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // Seed JockeyContract invitation with status 'Accepted'
                    var contract = await _context.JockeyContracts.FirstOrDefaultAsync(jc => jc.TournamentId == dacTour3.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (contract == null)
                    {
                        contract = new JockeyContract
                        {
                            TournamentId = dacTour3.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = dacTour3.StartDate ?? DateTime.UtcNow.AddDays(10),
                            EndDate = dacTour3.EndDate ?? DateTime.UtcNow.AddDays(20),
                            Status = "Accepted", // Jockey accepted the invitation
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{dacTour3Name}' ACCEPTED successfully.");
                    }
                    else if (contract.Status != "Accepted")
                    {
                        contract.Status = "Accepted";
                        _context.JockeyContracts.Update(contract);
                        await _context.SaveChangesAsync();
                    }

                    // Seed Tournament Registration
                    var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == dacTour3.TournamentId && r.HorseId == horse.HorseId);
                    if (registration == null)
                    {
                        registration = new Registration
                        {
                            TournamentId = dacTour3.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending", // Waiting for referee/admin approval
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{dacTour3Name}' seeded successfully.");
                    }
                }
            }

            // 6.7. Seed Tournament "Test Tournament 8 (Completed)" (Closed Tournament)
            var t8Name = "Test Tournament 8 (Completed)";
            var t8 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == t8Name);
            if (t8 == null)
            {
                t8 = new Tournament
                {
                    Name = t8Name,
                    Description = "Test Tournament 8 (Completed) - Already completed tournament with full results",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-30),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(-25),
                    StartDate = DateTime.UtcNow.AddDays(-24),
                    EndDate = DateTime.UtcNow.AddDays(-15),
                    Status = "Completed"
                };
                _context.Tournaments.Add(t8);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{t8Name}' seeded successfully.");

                var round = new Round
                {
                    TournamentId = t8.TournamentId,
                    Name = "Final",
                    RoundNumber = 1,
                    StartDate = t8.StartDate,
                    EndDate = t8.EndDate,
                    Status = "Completed"
                };
                _context.Rounds.Add(round);
                await _context.SaveChangesAsync();

                var race = new Race
                {
                    RoundId = round.RoundId,
                    Name = "Completed Race 1",
                    RaceDate = t8.StartDate.Value.AddDays(4),
                    DistanceMeter = 1200,
                    MaxLanes = 12,
                    Status = "Completed"
                };
                _context.Races.Add(race);
                await _context.SaveChangesAsync();

                var activeJockeys = await _context.JockeyProfiles.ToListAsync();
                var t8VetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "vet1") ?? await _context.Users.FirstOrDefaultAsync(u => u.Username == "vet");
                int t8VetUserId = t8VetUser?.UserId ?? 1;

                var times = new decimal[] { 65.20m, 66.10m, 67.50m, 68.30m, 69.00m, 70.20m, 71.80m, 73.50m };
                var positions = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                string winnerHorseName = "";

                for (int i = 1; i <= 8; i++)
                {
                    var horseName = $"Owner3-Horse{i}";
                    var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                    var jockeyProfile = activeJockeys[(i - 1) % activeJockeys.Count];
                    var jockeyUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == jockeyProfile.UserId);

                    if (horse != null && jockeyUser != null)
                    {
                        if (i == 1)
                        {
                            winnerHorseName = horse.Name;
                        }

                        // Registration
                        var registration = new Registration
                        {
                            TournamentId = t8.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Approved",
                            RegisteredAt = t8.RegistrationStartDate ?? DateTime.UtcNow.AddDays(-30)
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();

                        // Medical check
                        var medicalCheck = new MedicalCheckRecord
                        {
                            RegistrationId = registration.RegistrationId,
                            UserId = t8VetUserId,
                            CheckType = "Initial",
                            Weight = 450.0m,
                            Temperature = 38.2m,
                            HeartRate = 40,
                            DopingResult = "Negative",
                            MedicalResult = "Pass",
                            Notes = "Auto-seeded completed tournament check",
                            CheckedAt = t8.RegistrationStartDate.Value.AddDays(2)
                        };
                        _context.MedicalCheckRecords.Add(medicalCheck);

                        // Jockey Contract
                        var contract = new JockeyContract
                        {
                            TournamentId = t8.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockeyUser.UserId,
                            StartDate = t8.StartDate.Value,
                            EndDate = t8.EndDate.Value,
                            Status = "Accepted",
                            InvitationExpiredAt = t8.RegistrationStartDate.Value.AddDays(5),
                            CreatedAt = t8.RegistrationStartDate.Value
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();

                        // Race Entry
                        var entry = new RaceEntry
                        {
                            RaceId = race.RaceId,
                            RegistrationId = registration.RegistrationId,
                            JockeyId = jockeyProfile.JockeyId,
                            LaneNo = i,
                            Status = "Finished",
                            WinningProbability = 12.5m,
                            CurrentOdds = 2.0m,
                            FinishTime = times[i - 1],
                            FinishPosition = positions[i - 1]
                        };
                        _context.RaceEntries.Add(entry);
                        await _context.SaveChangesAsync();
                    }
                }

                // Race Result
                if (!string.IsNullOrEmpty(winnerHorseName))
                {
                    var result = new RaceResult
                    {
                        RaceId = race.RaceId,
                        Winner = winnerHorseName,
                        ResultRecordedAt = race.RaceDate.AddHours(1),
                        CreatedAt = race.RaceDate.AddHours(1)
                    };
                    _context.RaceResults.Add(result);
                    await _context.SaveChangesAsync();
                }
            }

            // 6.8. Auto-accept pending jockey contracts for "Giải Test 1"
            var pendingContracts1 = await _context.JockeyContracts
                .Where(jc => jc.TournamentId == t1.TournamentId && jc.Status == "Pending")
                .ToListAsync();

            if (pendingContracts1.Any())
            {
                foreach (var contract in pendingContracts1)
                {
                    contract.Status = "Accepted";
                    _logger.LogInformation($"Auto-accepting JockeyContract {contract.ContractId} for horse {contract.HorseId} and jockey {contract.JockeyId} in '{t1Name}'.");
                }
                await _context.SaveChangesAsync();
            }

            // 7. Auto-accept pending jockey contracts for "Giải Test 5"
            var pendingContracts = await _context.JockeyContracts
                .Where(jc => jc.TournamentId == t5.TournamentId && jc.Status == "Pending")
                .ToListAsync();

            if (pendingContracts.Any())
            {
                foreach (var contract in pendingContracts)
                {
                    contract.Status = "Accepted";
                    _logger.LogInformation($"Auto-accepting JockeyContract {contract.ContractId} for horse {contract.HorseId} and jockey {contract.JockeyId} in '{t5Name}'.");
                }
                await _context.SaveChangesAsync();
            }

            // 7.5. Auto-accept pending jockey contracts for "Giải Test 6"
            var pendingContracts6 = await _context.JockeyContracts
                .Where(jc => jc.TournamentId == t6.TournamentId && jc.Status == "Pending")
                .ToListAsync();

            if (pendingContracts6.Any())
            {
                foreach (var contract in pendingContracts6)
                {
                    contract.Status = "Accepted";
                    _logger.LogInformation($"Auto-accepting JockeyContract {contract.ContractId} for horse {contract.HorseId} and jockey {contract.JockeyId} in '{t6Name}'.");
                }
                await _context.SaveChangesAsync();
            }

            // 7.6. Auto-accept pending jockey contracts for "Giải Test 7"
            var pendingContracts7 = await _context.JockeyContracts
                .Where(jc => jc.TournamentId == t7.TournamentId && jc.Status == "Pending")
                .ToListAsync();

            if (pendingContracts7.Any())
            {
                foreach (var contract in pendingContracts7)
                {
                    contract.Status = "Accepted";
                    _logger.LogInformation($"Auto-accepting JockeyContract {contract.ContractId} for horse {contract.HorseId} and jockey {contract.JockeyId} in '{t7Name}'.");
                }
                await _context.SaveChangesAsync();
            }

            // 8. Auto-approve all registrations and seed passing MedicalCheckRecords for "Giải Test 1", "Giải Test 5", "Giải Test 6" and "Giải Test 7"
            var targetTournamentNames = new[] { "Test Tournament 1", "Test Tournament 5", "Test Tournament 6", "Test Tournament 7" };
            var targetTournaments = await _context.Tournaments
                .Where(t => targetTournamentNames.Contains(t.Name))
                .ToListAsync();

            var vet = await _context.Users.FirstOrDefaultAsync(u => u.Username == "vet1");
            int vetId = vet?.UserId ?? 1;

            foreach (var t in targetTournaments)
            {
                // Approve registrations
                var pendingRegistrations = await _context.Registrations
                    .Where(r => r.TournamentId == t.TournamentId && r.Status == "Pending")
                    .ToListAsync();

                foreach (var reg in pendingRegistrations)
                {
                    reg.Status = "Approved";
                    _logger.LogInformation($"Auto-approving Registration {reg.RegistrationId} for horse {reg.HorseId} in '{t.Name}'.");
                }
                await _context.SaveChangesAsync();

                // Create medical checks for all approved registrations
                var approvedRegistrations = await _context.Registrations
                    .Where(r => r.TournamentId == t.TournamentId && r.Status == "Approved")
                    .ToListAsync();

                foreach (var reg in approvedRegistrations)
                {
                    var hasMedicalCheck = await _context.MedicalCheckRecords
                        .AnyAsync(mc => mc.RegistrationId == reg.RegistrationId);

                    if (!hasMedicalCheck)
                    {
                        var medicalCheck = new MedicalCheckRecord
                        {
                            RegistrationId = reg.RegistrationId,
                            UserId = vetId,
                            Weight = 450.0m,
                            Temperature = 38.2m,
                            HeartRate = 40,
                            DopingResult = "Negative",
                            MedicalResult = "Pass",
                            Notes = "Auto-seeded passing check",
                            CheckedAt = DateTime.UtcNow
                        };
                        _context.MedicalCheckRecords.Add(medicalCheck);
                        _logger.LogInformation($"Auto-seeding passing MedicalCheckRecord for Registration {reg.RegistrationId} (Horse {reg.HorseId}) in '{t.Name}'.");
                    }
                }
                await _context.SaveChangesAsync();

                // 9. Auto-generate rounds, races, and entries for "Giải Test 1" if not exists
                if (t.Name == "Test Tournament 1")
                {
                    var hasRounds = await _context.Rounds.AnyAsync(r => r.TournamentId == t.TournamentId);
                    if (!hasRounds)
                    {
                        _logger.LogInformation($"Auto-generating rounds and races for '{t.Name}'...");
                        
                        var finalRound = new Round
                        {
                            TournamentId = t.TournamentId,
                            Name = "Final",
                            RoundNumber = 2,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            Status = "Scheduled"
                        };
                        _context.Rounds.Add(finalRound);
                        await _context.SaveChangesAsync();

                        var finalRace = new Race
                        {
                            RoundId = finalRound.RoundId,
                            Name = "Final Race",
                            RaceDate = t.EndDate ?? DateTime.UtcNow.AddDays(1),
                            DistanceMeter = 1600,
                            MaxLanes = 12,
                            Status = "Scheduled"
                        };
                        _context.Races.Add(finalRace);
                        await _context.SaveChangesAsync();

                        var activeJockeys = await _context.JockeyContracts
                            .Where(jc => jc.TournamentId == t.TournamentId && jc.Status == "Accepted")
                            .Join(_context.JockeyProfiles,
                                jc => jc.JockeyId,
                                jp => jp.UserId,
                                (jc, jp) => new { jc.HorseId, jp.JockeyId })
                            .ToDictionaryAsync(x => x.HorseId, x => x.JockeyId);

                        var entries = new List<RaceEntry>();
                        int lane = 1;
                        foreach (var reg in approvedRegistrations)
                        {
                            int? jockeyId = activeJockeys.TryGetValue(reg.HorseId, out var jId) ? jId : (int?)null;
                            
                            entries.Add(new RaceEntry
                            {
                                RaceId = finalRace.RaceId,
                                RegistrationId = reg.RegistrationId,
                                JockeyId = jockeyId,
                                LaneNo = lane++,
                                Status = "Confirmed"
                            });
                        }
                        _context.RaceEntries.AddRange(entries);
                        await _context.SaveChangesAsync();

                        // Now calculate the odds for finalRace entries using the new formula
                        var scores = new List<(RaceEntry entry, decimal speed, decimal jockey, decimal winRate)>();
                        foreach (var entry in entries)
                        {
                            var reg = approvedRegistrations.First(r => r.RegistrationId == entry.RegistrationId);
                            var horse = await _context.Horses.FindAsync(reg.HorseId);
                            
                            var avgSpeed = horse?.AverageTime ?? 15.0m;
                            if (avgSpeed <= 0) avgSpeed = 15.0m;

                            var recentAvgSpeed = horse?.RecentAverageTime ?? avgSpeed;
                            if (recentAvgSpeed <= 0) recentAvgSpeed = avgSpeed;

                            var combinedSpeed = 0.5m * avgSpeed + 0.5m * recentAvgSpeed;

                            var jockeyProfile = entry.JockeyId.HasValue ? await _context.JockeyProfiles.FirstOrDefaultAsync(jp => jp.JockeyId == entry.JockeyId.Value) : null;
                            var jockeyRank = jockeyProfile != null ? jockeyProfile.RankingPoint : 100;
                            if (jockeyRank <= 0) jockeyRank = 100;

                            var winRate = horse?.WinRate ?? 0.05m;
                            if (winRate <= 0) winRate = 0.05m;
                            if (winRate > 1) winRate /= 100m;

                            scores.Add((entry, combinedSpeed, (decimal)jockeyRank, winRate));
                        }

                        var totSpeed = scores.Sum(x => x.speed);
                        var totJockey = scores.Sum(x => x.jockey);
                        var totWin = scores.Sum(x => x.winRate);

                        foreach (var item in scores)
                        {
                            var relSpeed = totSpeed > 0 ? item.speed / totSpeed : 1.0m / scores.Count;
                            var relJockey = totJockey > 0 ? item.jockey / totJockey : 1.0m / scores.Count;
                            var relWin = totWin > 0 ? item.winRate / totWin : 1.0m / scores.Count;

                            var winProb = 0.5m * relSpeed + 0.3m * relJockey + 0.2m * relWin;
                            var winPct = winProb * 100m;
                            var odds = winProb > 0 ? 1.0m / winProb : 1.0m / (1.0m / scores.Count);
                            var finalOdds = Math.Max(odds * 0.9m, 1.05m);

                            item.entry.WinningProbability = Math.Round(winPct, 2);
                            item.entry.CurrentOdds = Math.Round(finalOdds, 2);
                        }
                        
                        t.Status = "Active";
                        _context.Tournaments.Update(t);
                        
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Auto-generated rounds, races, and entries (with calculated odds) for '{t.Name}' successfully.");
                    }
                }
            }

            // 6. Seed Tournaments "Block10" & "WC 2026"
            var targetTournamentsList = new[] { "Block10", "WC 2026" };
            foreach (var tName in targetTournamentsList)
            {
                var tBlock = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == tName);
                if (tBlock == null)
                {
                    tBlock = new Tournament
                    {
                        Name = tName,
                        Description = $"{tName} Tournament Description",
                        RegistrationStartDate = DateTime.UtcNow.AddDays(-5),
                        RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                        StartDate = DateTime.UtcNow.AddDays(6),
                        EndDate = DateTime.UtcNow.AddDays(15),
                        Status = "Registration Open"
                    };
                    _context.Tournaments.Add(tBlock);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Tournament '{tName}' seeded successfully.");
                }

                var vet = await _context.Users.FirstOrDefaultAsync(u => u.Username == "vet");
                int vetId = vet?.UserId ?? 1;

                int limit = tName == "WC 2026" ? 13 : 12;
                for (int i = 1; i <= limit; i++)
                {
                    var horseName = $"Owner3-Horse{i}";
                    var jockeyUsername = $"jockeytest{i}";

                    var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                    var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                    if (horse != null && jockey != null)
                    {
                        // Ensure horse is Healthy so that entering race results won't fail due to previous tests
                        if (horse.HealthStatus != "Healthy")
                        {
                            horse.HealthStatus = "Healthy";
                            _context.Horses.Update(horse);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Horse '{horseName}' health status set to Healthy.");
                        }

                        // 6a. Seed JockeyContract invitation
                        var hasContract = await _context.JockeyContracts.AnyAsync(jc => jc.TournamentId == tBlock.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                        if (!hasContract)
                        {
                            var contract = new JockeyContract
                            {
                                TournamentId = tBlock.TournamentId,
                                HorseId = horse.HorseId,
                                JockeyId = jockey.UserId,
                                StartDate = tBlock.StartDate ?? DateTime.UtcNow.AddDays(6),
                                EndDate = tBlock.EndDate ?? DateTime.UtcNow.AddDays(15),
                                Status = "Accepted",
                                InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.JockeyContracts.Add(contract);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{tName}' seeded successfully.");
                        }

                        // 6b. Seed Tournament Registration
                        var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == tBlock.TournamentId && r.HorseId == horse.HorseId);
                        if (registration == null)
                        {
                            registration = new Registration
                            {
                                TournamentId = tBlock.TournamentId,
                                HorseId = horse.HorseId,
                                Status = "Approved",
                                RegisteredAt = DateTime.UtcNow
                            };
                            _context.Registrations.Add(registration);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Registration for horse '{horseName}' in '{tName}' seeded successfully.");
                        }
                        else if (registration.Status != "Approved")
                        {
                            registration.Status = "Approved";
                            _context.Registrations.Update(registration);
                            await _context.SaveChangesAsync();
                        }

                        // 6c. Seed passing MedicalCheckRecord
                        var hasMedicalCheck = await _context.MedicalCheckRecords.AnyAsync(mc => mc.RegistrationId == registration.RegistrationId);
                        if (!hasMedicalCheck)
                        {
                            var medicalCheck = new MedicalCheckRecord
                            {
                                RegistrationId = registration.RegistrationId,
                                UserId = vetId,
                                Weight = 450.0m,
                                Temperature = 38.2m,
                                HeartRate = 40,
                                DopingResult = "Negative",
                                MedicalResult = "Pass",
                                Notes = $"Auto-seeded passing check for {tName}",
                                CheckedAt = DateTime.UtcNow
                            };
                            _context.MedicalCheckRecords.Add(medicalCheck);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"MedicalCheckRecord for Registration {registration.RegistrationId} in '{tName}' seeded successfully.");
                        }
                    }
                }

                // 6d. Automatically generate Rounds, Races, and Assignments for Spectator Betting flow
                var rounds = await _context.Rounds.Where(r => r.TournamentId == tBlock.TournamentId).ToListAsync();
                if (!rounds.Any())
                {
                    if (limit == 12)
                    {
                        // Final Round
                        var finalRound = new Round
                        {
                            TournamentId = tBlock.TournamentId,
                            Name = "Final",
                            RoundNumber = 2,
                            StartDate = tBlock.StartDate,
                            EndDate = tBlock.EndDate,
                            Status = "Scheduled"
                        };
                        _context.Rounds.Add(finalRound);
                        await _context.SaveChangesAsync();

                        var finalRace = new Race
                        {
                            RoundId = finalRound.RoundId,
                            Name = "Final Race",
                            RaceDate = tBlock.EndDate ?? DateTime.UtcNow.AddDays(10),
                            DistanceMeter = 1600,
                            MaxLanes = 12,
                            Status = "Scheduled"
                        };
                        _context.Races.Add(finalRace);
                        await _context.SaveChangesAsync();

                        // Referee assignment
                        var defaultRef = await _context.Users.FirstOrDefaultAsync(u => u.Username == "referee");
                        if (defaultRef != null)
                        {
                            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                            {
                                RaceId = finalRace.RaceId,
                                RefereeId = defaultRef.UserId,
                                AssignedAt = DateTime.UtcNow,
                                Status = "Active"
                            });
                            await _context.SaveChangesAsync();
                        }

                        // Race Entries
                        int lane = 1;
                        for (int i = 1; i <= limit; i++)
                        {
                            var horseName = $"Owner3-Horse{i}";
                            var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                            var jockeyUsername = $"jockeytest{i}";
                            var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);
                            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == tBlock.TournamentId && r.HorseId == horse.HorseId);
                            if (registration != null)
                            {
                                _context.RaceEntries.Add(new RaceEntry
                                {
                                    RaceId = finalRace.RaceId,
                                    RegistrationId = registration.RegistrationId,
                                    JockeyId = jockey?.UserId,
                                    LaneNo = lane++,
                                    Status = "Confirmed",
                                    WinningProbability = 0.5m,
                                    CurrentOdds = 2.0m
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Round and Race seeded successfully for Final round tournament {tName}.");
                    }
                    else
                    {
                        // Pre Round
                        var preRound = new Round
                        {
                            TournamentId = tBlock.TournamentId,
                            Name = "Pre",
                            RoundNumber = 1,
                            StartDate = tBlock.StartDate,
                            EndDate = tBlock.EndDate,
                            Status = "Scheduled"
                        };
                        _context.Rounds.Add(preRound);
                        await _context.SaveChangesAsync();

                        // Race 1 (7 horses)
                        var preRace1 = new Race
                        {
                            RoundId = preRound.RoundId,
                            Name = "Pre Round Race 1",
                            RaceDate = tBlock.StartDate ?? DateTime.UtcNow.AddDays(7),
                            DistanceMeter = 1600,
                            MaxLanes = 12,
                            Status = "Scheduled"
                        };
                        _context.Races.Add(preRace1);
                        await _context.SaveChangesAsync();

                        // Race 2 (6 horses)
                        var preRace2 = new Race
                        {
                            RoundId = preRound.RoundId,
                            Name = "Pre Round Race 2",
                            RaceDate = tBlock.StartDate ?? DateTime.UtcNow.AddDays(7),
                            DistanceMeter = 1600,
                            MaxLanes = 12,
                            Status = "Scheduled"
                        };
                        _context.Races.Add(preRace2);
                        await _context.SaveChangesAsync();

                        // Referee assignments
                        var defaultRef = await _context.Users.FirstOrDefaultAsync(u => u.Username == "referee");
                        if (defaultRef != null)
                        {
                            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                            {
                                RaceId = preRace1.RaceId,
                                RefereeId = defaultRef.UserId,
                                AssignedAt = DateTime.UtcNow,
                                Status = "Active"
                            });
                            _context.RaceRefereeAssignments.Add(new RaceRefereeAssignment
                            {
                                RaceId = preRace2.RaceId,
                                RefereeId = defaultRef.UserId,
                                AssignedAt = DateTime.UtcNow,
                                Status = "Active"
                            });
                            await _context.SaveChangesAsync();
                        }

                        // Race 1 entries (1 to 7)
                        int lane = 1;
                        for (int i = 1; i <= 7; i++)
                        {
                            var horseName = $"Owner3-Horse{i}";
                            var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                            var jockeyUsername = $"jockeytest{i}";
                            var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);
                            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == tBlock.TournamentId && r.HorseId == horse.HorseId);
                            if (registration != null)
                            {
                                _context.RaceEntries.Add(new RaceEntry
                                {
                                    RaceId = preRace1.RaceId,
                                    RegistrationId = registration.RegistrationId,
                                    JockeyId = jockey?.UserId,
                                    LaneNo = lane++,
                                    Status = "Confirmed",
                                    WinningProbability = 0.5m,
                                    CurrentOdds = 2.0m
                                });
                            }
                        }

                        // Race 2 entries (8 to 13)
                        lane = 1;
                        for (int i = 8; i <= 13; i++)
                        {
                            var horseName = $"Owner3-Horse{i}";
                            var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                            var jockeyUsername = $"jockeytest{i}";
                            var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);
                            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == tBlock.TournamentId && r.HorseId == horse.HorseId);
                            if (registration != null)
                            {
                                _context.RaceEntries.Add(new RaceEntry
                                {
                                    RaceId = preRace2.RaceId,
                                    RegistrationId = registration.RegistrationId,
                                    JockeyId = jockey?.UserId,
                                    LaneNo = lane++,
                                    Status = "Confirmed",
                                    WinningProbability = 0.5m,
                                    CurrentOdds = 2.0m
                                });
                            }
                        }
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Pre round races seeded successfully for tournament {tName}.");
                    }
                }
            }

            _logger.LogInformation("Mandatory data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding mandatory data.");
            throw;
        }
    }
}