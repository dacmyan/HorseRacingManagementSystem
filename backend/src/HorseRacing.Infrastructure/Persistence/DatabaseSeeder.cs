using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;


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
            new { Username = "admin",    Email = "admin@gmail.com",    FullName = "Admin",                 RoleId = 1, Status = "Active" },
            new { Username = "owner",    Email = "owner@gmail.com",    FullName = "Nguyễn Văn Hùng",      RoleId = 2, Status = "Active" },
            new { Username = "owner2",   Email = "owner2@gmail.com",   FullName = "Trần Thị Mai",           RoleId = 2, Status = "Active" },
            new { Username = "owner3",   Email = "owner3@gmail.com",   FullName = "Lê Minh Tuấn",            RoleId = 2, Status = "Active" },
            new { Username = "jockey",   Email = "jockey@gmail.com",   FullName = "Jockey",                RoleId = 3, Status = "Active" },
            new { Username = "referee",  Email = "referee@gmail.com",  FullName = "Referee",               RoleId = 4, Status = "Active" },
            new { Username = "spectator",Email = "spectator@gmail.com", FullName = "Spectator",            RoleId = 5, Status = "Active" }
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

        // Seed 20 Referees dynamically
        for (int i = 1; i <= 20; i++)
        {
            var username = $"referee{i}";
            var email = $"referee{i}@gmail.com";
            var fullName = $"Trọng tài {i}";
            if (!await _context.Users.AnyAsync(u => u.Email == email || u.Username == username))
            {
                var user = new AppUser
                {
                    Username = username,
                    Email = email,
                    FullName = fullName,
                    RoleId = 4, // Referee
                    Status = "Active",
                    CreatedAt = fixedDate
                };
                user.PasswordHash = hasher.HashPassword(user, "123456");

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (!await _context.RefereeProfiles.AnyAsync(rp => rp.UserId == user.UserId))
                {
                    _context.RefereeProfiles.Add(new RefereeProfile
                    {
                        UserId = user.UserId,
                        LicenseNumber = $"LIC-REF-{i:D3}",
                        ExperienceYears = 3 + (i % 8),
                        Status = "Active"
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        // Seed violations only when a race already exists. Do not create a
        // synthetic tournament/race here; admins need a clean slate for tests.
        var anyRace = await _context.Races.FirstOrDefaultAsync();
        if (anyRace != null && !await _context.Violations.AnyAsync())
        {
            var violationsData = new[]
            {
                new { Description = "Chạy sai làn: Ngũa chạy lấn làn số 3", Penalty = "Cảnh cáo", Status = "Pending" },
                new { Description = "Cản trở đối thủ: Cố tình ép xe ngựa số 5", Penalty = "Trừ 5 điểm", Status = "Pending" },
                new { Description = "Xuất phát sớm: Di chuyển trước khi có hiệu lệnh", Penalty = "Cảnh cáo", Status = "Pending" },
                new { Description = "Vi phạm thiết bị: Roi da không đúng quy chuẩn", Penalty = "Phạt 2.000.000 VND", Status = "Confirmed" },
                new { Description = "Cản trở đối thủ: Đè ngựa đối thủ góc cua số 2", Penalty = "Cấm thi đấu 1 trận", Status = "Confirmed" },
                new { Description = "Không tuân thủ hiệu lệnh: Bất tuân hiệu lệnh trọng tài", Penalty = "Hủy kết quả", Status = "Confirmed" },
                new { Description = "Kháng cáo: Yêu cầu xem lại camera góc cua số 4", Penalty = "Không phạt", Status = "Rejected" },
                new { Description = "Khiếu nại: Nghi ngờ ngựa số 8 sử dụng chất kích thích", Penalty = "Đang xem xét", Status = "Rejected" }
            };

            foreach (var vData in violationsData)
            {
                _context.Violations.Add(new RaceViolation
                {
                    RaceId = anyRace.RaceId,
                    Description = vData.Description,
                    Penalty = vData.Penalty,
                    Status = vData.Status
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}

