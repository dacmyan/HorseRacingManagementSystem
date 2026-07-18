using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HorseRacing.Infrastructure.Persistence;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
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

                        // Quét các giải đấu đã hết hạn đăng ký nhưng ở trạng thái "PendingRegistration"
                        var expiredTournaments = await context.Tournaments
                            .Where(t => t.RegistrationEndDate != null && now > t.RegistrationEndDate && (t.Status == "PendingRegistration" || t.Status == "Registration Open"))
                            .ToListAsync(stoppingToken);

                        foreach (var tournament in expiredTournaments)
                        {
                            int registeredCount = await context.Registrations
                                .CountAsync(r => r.TournamentId == tournament.TournamentId, stoppingToken);

                            if (registeredCount >= 12)
                            {
                                // Đủ 12 ngựa -> Đóng đăng ký bình thường và chuyển sang trạng thái "PendingScheduling" (Chờ xếp lịch)
                                tournament.Status = "PendingScheduling";
                                await context.SaveChangesAsync(stoppingToken);
                                Console.WriteLine($"[SYSTEM AUTOMATION]: Tournament {tournament.TournamentId} ({tournament.Name}) has {registeredCount} registered horses. Status updated to PendingScheduling.");

                                // Auto-cancel registrations without accepted jockey
                                var tournamentRepo = scope.ServiceProvider.GetRequiredService<ITournamentRepository>();
                                var cancelledRegs = await tournamentRepo.CancelRegistrationsWithoutJockeyAsync(tournament.TournamentId);

                                if (cancelledRegs.Count > 0)
                                {
                                    var notificationService = scope.ServiceProvider.GetService<INotificationService>();
                                    if (notificationService != null)
                                    {
                                        // Group by owner to send one notification per owner
                                        var ownerGroups = cancelledRegs.GroupBy(c => c.OwnerId);
                                        foreach (var group in ownerGroups)
                                        {
                                            var horseNames = string.Join(", ", group.Select(c => c.HorseName));
                                            var tournamentName = group.First().TournamentName;
                                            try
                                            {
                                                await notificationService.SendNotificationToUserAsync(
                                                    group.Key,
                                                    "Đăng ký bị hủy tự động",
                                                    $"Đăng ký của ngựa [{horseNames}] trong giải đấu '{tournamentName}' đã bị hủy tự động do chưa có jockey được chấp nhận khi đăng ký đóng.",
                                                    "System",
                                                    (int)tournament.TournamentId,
                                                    actionUrl: "/owner/registrations"
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"[NOTIFICATION ERROR] Failed to send auto-cancel notification to owner {group.Key}: {ex.Message}");
                                            }
                                        }
                                    }
                                    Console.WriteLine($"[SYSTEM AUTOMATION]: {cancelledRegs.Count} registration(s) auto-cancelled for Tournament {tournament.TournamentId} ({tournament.Name}) due to missing jockey.");
                                }
                            }
                            else
                            {
                                // Không đủ 12 ngựa
                                if (tournament.CancelCount == 0)
                                {
                                    // Lần 1: Tự động gia hạn thêm 3 ngày
                                    tournament.RegistrationEndDate = tournament.RegistrationEndDate.Value.AddDays(3);
                                    if (tournament.StartDate != null)
                                        tournament.StartDate = tournament.StartDate.Value.AddDays(3);
                                    if (tournament.EndDate != null)
                                        tournament.EndDate = tournament.EndDate.Value.AddDays(3);
                                    tournament.CancelCount = 1;
                                    await context.SaveChangesAsync(stoppingToken);
                                    Console.WriteLine($"[SYSTEM AUTOMATION]: Tournament {tournament.TournamentId} ({tournament.Name}) has only {registeredCount} registrations. Extended deadline by 3 days.");
                                }
                                else if (tournament.CancelCount == 1)
                                {
                                    // Lần 2: Hủy giải đấu
                                    tournament.Status = "Cancelled";
                                    tournament.CancelCount = 2;
                                    await context.SaveChangesAsync(stoppingToken);
                                    Console.WriteLine($"[SYSTEM AUTOMATION]: Tournament {tournament.TournamentId} ({tournament.Name}) failed to reach 12 registrations after extension. Status updated to Cancelled.");

                                    // Lấy danh sách OwnerId duy nhất của các ngựa đã đăng ký
                                    var owners = await context.Registrations
                                        .Where(r => r.TournamentId == tournament.TournamentId && r.Horse != null)
                                        .Select(r => r.Horse.OwnerId)
                                        .Distinct()
                                        .ToListAsync(stoppingToken);

                                    var notificationService = scope.ServiceProvider.GetService<INotificationService>();
                                    if (notificationService != null)
                                    {
                                        foreach (var ownerId in owners)
                                        {
                                            try
                                            {
                                                await notificationService.SendNotificationToUserAsync(
                                                    ownerId,
                                                    "Giải đấu bị hủy",
                                                    $"Giải đấu '{tournament.Name}' đã bị hủy do không đủ số lượng ngựa đăng ký tối thiểu (12 ngựa) sau thời gian gia hạn.",
                                                    "System",
                                                    (int)tournament.TournamentId
                                                );
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"[NOTIFICATION ERROR] Failed to send notification to owner {ownerId}: {ex.Message}");
                                            }
                                        }
                                    }
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
