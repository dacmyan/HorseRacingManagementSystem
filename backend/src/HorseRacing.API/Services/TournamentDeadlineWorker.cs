using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HorseRacing.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HorseRacing.API.Services
{
    public class TournamentDeadlineWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TournamentDeadlineWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var now = DateTime.UtcNow;

                        // Quét các giải đấu đã hết hạn đăng ký thực tế nhưng chưa được xử lý triệt để
                        var expiredTournaments = await context.Tournaments
                            .Where(t => t.RegistrationEndDate != null && now > t.RegistrationEndDate && t.Status == "Registration Open")
                            .ToListAsync(stoppingToken);

                        foreach (var tournament in expiredTournaments)
                        {
                            if (tournament.CancelCount == 0)
                            {
                                // LẦN 1 HẾT HẠN: Tạm đóng và thông báo cho Admin
                                tournament.Status = "Registration Suspended";
                                await context.SaveChangesAsync(stoppingToken);
                                
                                // Call Service: Gửi thông báo đến Admin hệ thống (Giả lập logs hệ thống)
                                Console.WriteLine($"[NOTIFICATION TO ADMIN]: Giải đấu {tournament.TournamentId} hết hạn đăng ký lần đầu. Vui lòng thực hiện gia hạn.");
                            }
                            else if (tournament.CancelCount == 1)
                            {
                                // LẦN 2 HẾT HẠN: Kiểm tra số lượng ngựa tối thiểu (Giả định kiểm tra đơn đăng ký hợp lệ)
                                int approvedOrPendingCount = await context.Registrations
                                    .CountAsync(r => r.TournamentId == tournament.TournamentId, stoppingToken);

                                if (approvedOrPendingCount < 12) // Giới hạn tối thiểu 12 con ngựa dựa trên kịch bản đua
                                {
                                    // Chuyển trạng thái giải đấu sang Hủy
                                    tournament.Status = "Cancelled";
                                    tournament.CancelCount = 2;
                                    await context.SaveChangesAsync(stoppingToken);

                                    // Lấy danh sách email của tất cả các chủ ngựa đăng ký vào giải đấu này
                                    // Giả lập logic truy vấn thông tin Email của Owner đăng ký
                                    Console.WriteLine($"[SYSTEM AUTOMATION]: Giải đấu {tournament.TournamentId} chính thức bị HỦY hoàn toàn.");
                                    Console.WriteLine($"[EMAIL SERVICE]: Đang gửi email thông báo hủy giải đấu đến tất cả các HorseOwner đã đăng ký...");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error in TournamentDeadlineWorker: {ex.Message}");
                }

                // Quét định kỳ mỗi giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
