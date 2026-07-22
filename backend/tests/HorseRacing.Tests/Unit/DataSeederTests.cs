using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HorseRacing.Tests.Unit;

public class DataSeederTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Seed basic roles
        context.Roles.AddRange(
            new Role { RoleId = 1, Name = "Admin" },
            new Role { RoleId = 2, Name = "HorseOwner" },
            new Role { RoleId = 3, Name = "Jockey" },
            new Role { RoleId = 4, Name = "Referee" },
            new Role { RoleId = 5, Name = "Spectator" },
            new Role { RoleId = 6, Name = "Veterinarian" }
        );

        // Pre-seed protected existing users to verify they are untouched
        var hasher = new PasswordHasher<AppUser>();
        var existingUsers = new[] { "admin", "owner", "jockey", "spectator", "referee", "jockey-1", "vet" };
        foreach (var uname in existingUsers)
        {
            var user = new AppUser
            {
                Username = uname,
                Email = $"{uname}@gmail.com",
                FullName = $"Existing {uname}",
                RoleId = 1,
                Status = "Active",
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, "existingpass");
            context.Users.Add(user);
        }

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task SeedAppUsersAsync_ShouldCreateRequiredAccountsWithCorrectPasswordHash()
    {
        // Arrange
        using var context = CreateDbContext();
        var seeder = new DataSeeder(context, NullLogger<DataSeeder>.Instance);
        var hasher = new PasswordHasher<AppUser>();

        // Act
        await seeder.SeedAsync();

        // Assert - Existing pre-seeded accounts exist
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        admin.Should().NotBeNull();

        var vet = await context.Users.FirstOrDefaultAsync(u => u.Username == "vet");
        vet.Should().NotBeNull();

        var referee = await context.Users.FirstOrDefaultAsync(u => u.Username == "referee");
        referee.Should().NotBeNull();

        var owner3 = await context.Users.FirstOrDefaultAsync(u => u.Username == "owner3");
        owner3.Should().NotBeNull();
        owner3!.RoleId.Should().Be(2);

        // Assert - Jockeys: jockeytest1 .. jockeytest25
        for (int i = 1; i <= 25; i++)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == $"jockeytest{i}");
            user.Should().NotBeNull();
            user!.Email.Should().Be($"jockeytest{i}@gmail.com");
            user.FullName.Should().Be($"Jockey-Test{i}");
            user.RoleId.Should().Be(3);
            user.Status.Should().Be("Active");
            user.IsEmailConfirmed.Should().BeTrue();

            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, "123456");
            verifyResult.Should().Be(PasswordVerificationResult.Success);

            var profile = await context.JockeyProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            profile.Should().NotBeNull();
        }

        // Assert - Existing protected accounts were not modified
        var existingJockey1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "jockey-1");
        existingJockey1.Should().NotBeNull();
        existingJockey1!.FullName.Should().Be("Existing jockey-1");
        hasher.VerifyHashedPassword(existingJockey1, existingJockey1.PasswordHash, "existingpass")
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task SeedAppUsersAsync_ShouldBeIdempotent()
    {
        // Arrange
        using var context = CreateDbContext();
        var seeder = new DataSeeder(context, NullLogger<DataSeeder>.Instance);

        // Act
        await seeder.SeedAsync();
        var countFirstRun = await context.Users.CountAsync();

        await seeder.SeedAsync();
        var countSecondRun = await context.Users.CountAsync();

        // Assert
        countSecondRun.Should().Be(countFirstRun);
    }
}
