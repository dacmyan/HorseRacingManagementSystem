using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;

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
                    CreatedAt = DateTime.UtcNow
                };
                vetUser.PasswordHash = hasher.HashPassword(vetUser, "123456");

                _context.Users.Add(vetUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Default Veterinarian user ('vet@gmail.com' / '123456') seeded successfully.");
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