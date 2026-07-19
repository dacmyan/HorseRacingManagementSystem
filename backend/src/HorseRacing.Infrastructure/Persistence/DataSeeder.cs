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
                    var horse = new Horse
                    {
                        Name = horseName,
                        Age = DateTime.UtcNow.AddYears(-5),
                        Gender = i % 2 == 0 ? "Stallion" : "Mare",
                        Breed = "Thoroughbred",
                        HealthStatus = "Healthy",
                        OwnerId = owner3User.UserId,
                        AverageTime = 68.00m,
                        RecentAverageTime = 68.00m,
                        WinRate = 0.50m
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
                    _logger.LogInformation($"Test Horse '{horseName}' seeded successfully.");
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