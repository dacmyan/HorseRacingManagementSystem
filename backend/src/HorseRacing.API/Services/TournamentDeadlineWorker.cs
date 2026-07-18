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
        private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

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
                        var now = VietnamNow;
                        var notificationService = scope.ServiceProvider.GetService<INotificationService>();

                        // 1. Quét các giải đấu cần chuyển sang trạng thái "Registration Open"
                        var pendingOpenTournaments = await context.Tournaments
                            .Where(t => t.Status == "PendingRegistration" && t.RegistrationStartDate != null && now >= t.RegistrationStartDate)
                            .ToListAsync(stoppingToken);

                        foreach (var tournament in pendingOpenTournaments)
                        {
                            tournament.Status = "Registration Open";
                            await context.SaveChangesAsync(stoppingToken);
                            Console.WriteLine($"[SYSTEM AUTOMATION]: Tournament {tournament.TournamentId} ({tournament.Name}) is now open for registration. Status updated to Registration Open.");

                            if (notificationService != null)
                            {
                                try
                                {
                                    await notificationService.BroadcastNotificationAsync(
                                        "New Tournament Open for Registration",
                                        $"Tournament '{tournament.Name}' starting on {tournament.StartDate:dd/MM/yyyy} is now open for registration.",
                                        "Tournament",
                                        referenceId: (int)tournament.TournamentId,
                                        actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[NOTIFICATION ERROR] Failed to broadcast tournament open: {ex.Message}");
                                }
                            }
                        }

                        // 2. Quét các giải đấu đã hết hạn đăng ký nhưng ở trạng thái "PendingRegistration" hoặc "Registration Open"
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

                // Quét định kỳ mỗi phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
