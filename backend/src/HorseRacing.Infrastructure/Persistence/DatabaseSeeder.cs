using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // 1. Seed Roles (Thường đã được chạy qua EF HasData, nhưng vẫn thêm để an toàn tuyệt đối)
        var roles = new[]
        {
            new Role { RoleId = 1, Name = "Admin" },
            new Role { RoleId = 2, Name = "HorseOwner" },
            new Role { RoleId = 3, Name = "Jockey" },
            new Role { RoleId = 4, Name = "Referee" },
            new Role { RoleId = 5, Name = "Spectator" }
        };

        foreach (var role in roles)
        {
            if (!await _context.Roles.AnyAsync(r => r.RoleId == role.RoleId))
            {
                // Tránh lỗi chèn cột Identity trong EF Core bằng cách kích hoạt tạm thời IDENTITY_INSERT nếu cần thiết.
                // Tuy nhiên do EF HasData đã tạo Roles nên ở đây chỉ phòng hờ nếu DB rỗng hoàn toàn.
                _context.Roles.Add(role);
            }
        }
        await _context.SaveChangesAsync();

        // 2. Seed Users ở runtime để băm mật khẩu động (Dynamic Password Hashing)
        var hasher = new PasswordHasher<AppUser>();
        var fixedDate = new DateTime(2026, 6, 9, 0, 0, 0, DateTimeKind.Utc);

        var usersData = new[]
        {
            new { Username = "admin", Email = "admin@gmail.com", FullName = "Admin", RoleId = 1, Status = "Active" },
            new { Username = "owner", Email = "owner@gmail.com", FullName = "HorseOwner", RoleId = 2, Status = "Active" },
            new { Username = "jockey", Email = "jockey@gmail.com", FullName = "Jockey", RoleId = 3, Status = "Active" },
            new { Username = "referee", Email = "referee@gmail.com", FullName = "Referee", RoleId = 4, Status = "Active" },
            new { Username = "spectator", Email = "spectator@gmail.com", FullName = "Spectator", RoleId = 5, Status = "Active" }
        };

        foreach (var data in usersData)
        {
            if (!await _context.Users.AnyAsync(u => u.Email == data.Email || u.Username == data.Username))
            {
                var user = new AppUser
                {
                    Username = data.Username,
                    Email = data.Email,
                    FullName = data.FullName,
                    RoleId = data.RoleId,
                    Status = data.Status,
                    CreatedAt = fixedDate
                };
                user.PasswordHash = hasher.HashPassword(user, "123456");

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Save to generate UserId from SQL Server Identity

                // 3. Khởi tạo Profile / Wallet liên quan dựa trên vai trò
                if (data.RoleId == 3) // Jockey
                {
                    if (!await _context.JockeyProfiles.AnyAsync(jp => jp.UserId == user.UserId))
                    {
                        _context.JockeyProfiles.Add(new JockeyProfile
                        {
                            UserId = user.UserId,
                            ExperienceYears = 3,
                            RankingPoint = 100,
                            Status = "Active"
                        });
                    }
                }
                else if (data.RoleId == 4) // Referee
                {
                    if (!await _context.RefereeProfiles.AnyAsync(rp => rp.UserId == user.UserId))
                    {
                        _context.RefereeProfiles.Add(new RefereeProfile
                        {
                            UserId = user.UserId,
                            LicenseNumber = "LIC-REF-001",
                            ExperienceYears = 5,
                            Status = "Active"
                        });
                    }
                }
                else if (data.RoleId == 5) // Spectator
                {
                    if (!await _context.Wallets.AnyAsync(w => w.UserId == user.UserId))
                    {
                        _context.Wallets.Add(new Wallet
                        {
                            UserId = user.UserId,
                            Balance = 0
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
