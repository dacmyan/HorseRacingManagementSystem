using HorseRacing.Domain.Entities;
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
    }
}