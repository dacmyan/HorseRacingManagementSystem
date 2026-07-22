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
        await seeder.SeedAppUsersAsync();

        // Assert - Referees: referee-1 .. referee-4
        for (int i = 1; i <= 4; i++)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == $"referee-{i}");
            user.Should().NotBeNull();
            user!.Email.Should().Be($"referee-{i}@gmail.com");
            user.FullName.Should().Be($"Referee {i}");
            user.RoleId.Should().Be(4);
            user.Status.Should().Be("Active");
            user.IsEmailConfirmed.Should().BeTrue();

            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, "1-6");
            verifyResult.Should().Be(PasswordVerificationResult.Success);

            var profile = await context.RefereeProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            profile.Should().NotBeNull();
        }

        // Assert - Jockeys: jockey-2 .. jockey-49
        for (int i = 2; i <= 49; i++)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == $"jockey-{i}");
            user.Should().NotBeNull();
            user!.Email.Should().Be($"jockey-{i}@gmail.com");
            user.FullName.Should().Be($"Jockey {i}");
            user.RoleId.Should().Be(3);
            user.Status.Should().Be("Active");
            user.IsEmailConfirmed.Should().BeTrue();

            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, "1-6");
            verifyResult.Should().Be(PasswordVerificationResult.Success);

            var profile = await context.JockeyProfiles.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            profile.Should().NotBeNull();
        }

        // Assert - Owners: owner-1 .. owner-4
        for (int i = 1; i <= 4; i++)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == $"owner-{i}");
            user.Should().NotBeNull();
            user!.Email.Should().Be($"owner-{i}@gmail.com");
            user.FullName.Should().Be($"Owner {i}");
            user.RoleId.Should().Be(2);
            user.Status.Should().Be("Active");
            user.IsEmailConfirmed.Should().BeTrue();

            var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, "1-6");
            verifyResult.Should().Be(PasswordVerificationResult.Success);
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
        await seeder.SeedAppUsersAsync();
        var countFirstRun = await context.Users.CountAsync();

        await seeder.SeedAppUsersAsync();
        var countSecondRun = await context.Users.CountAsync();

        // Assert
        countSecondRun.Should().Be(countFirstRun);
    }
}
