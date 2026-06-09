using HorseRacing.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<AppUser>();
        
        var admin = new AppUser
        {
            Id = 1,
            Username = "admin",
            Email = "admin@gmail.com",
            FullName = "Admin",
            Role = "Admin"
        };
        admin.PasswordHash = hasher.HashPassword(admin, "123456");

        modelBuilder.Entity<AppUser>().HasData(admin);
    }
}