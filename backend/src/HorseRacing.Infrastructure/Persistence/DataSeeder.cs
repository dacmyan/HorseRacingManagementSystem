using HorseRacing.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        // 1. Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, Name = "Admin" },
            new Role { RoleId = 2, Name = "HorseOwner" },
            new Role { RoleId = 3, Name = "Jockey" },
            new Role { RoleId = 4, Name = "Referee" },
            new Role { RoleId = 5, Name = "Spectator" }
        );

        // 2. Seed Users
        var hasher = new PasswordHasher<AppUser>();
        
        var admin = new AppUser
        {
            UserId = 1,
            Username = "admin",
            Email = "admin@gmail.com",
            FullName = "Admin",
            RoleId = 1, // Admin role
            Status = "Active",
            CreatedAt = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc)
        };
        admin.PasswordHash = hasher.HashPassword(admin, "123456");

        var owner = new AppUser
        {
            UserId = 2,
            Username = "owner",
            Email = "owner@gmail.com",
            FullName = "HorseOwner",
            RoleId = 2,
            Status = "Active",
            CreatedAt = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc)
        };
        owner.PasswordHash = hasher.HashPassword(owner, "123456");

        var jockey = new AppUser
        {
            UserId = 3,
            Username = "jockey",
            Email = "jockey@gmail.com",
            FullName = "Jockey",
            RoleId = 3,
            Status = "Active",
            CreatedAt = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc)
        };
        jockey.PasswordHash = hasher.HashPassword(jockey, "123456");

        var referee = new AppUser
        {
            UserId = 4,
            Username = "referee",
            Email = "referee@gmail.com",
            FullName = "Referee",
            RoleId = 4,
            Status = "Active",
            CreatedAt = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc)
        };
        referee.PasswordHash = hasher.HashPassword(referee, "123456");

        var spectator = new AppUser
        {
            UserId = 5,
            Username = "spectator",
            Email = "spectator@gmail.com",
            FullName = "Spectator",
            RoleId = 5,
            Status = "Active",
            CreatedAt = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc)
        };
        spectator.PasswordHash = hasher.HashPassword(spectator, "123456");

        modelBuilder.Entity<AppUser>().HasData(admin, owner, jockey, referee, spectator);

        // 3. Seed Profiles / Wallets
        modelBuilder.Entity<JockeyProfile>().HasData(
            new JockeyProfile
            {
                JockeyId = 1,
                UserId = 3,
                ExperienceYears = 3,
                RankingPoint = 100,
                Status = "Active"
            }
        );

        modelBuilder.Entity<RefereeProfile>().HasData(
            new RefereeProfile
            {
                RefereeId = 1,
                UserId = 4,
                LicenseNumber = "LIC-REF-001",
                ExperienceYears = 5,
                Status = "Active"
            }
        );

        modelBuilder.Entity<Wallet>().HasData(
            new Wallet
            {
                WalletId = 1,
                UserId = 5,
                Balance = 0
            }
        );
    }
}