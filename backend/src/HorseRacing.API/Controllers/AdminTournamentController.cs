using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HorseRacing.Infrastructure.Persistence;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.Notifications.Interfaces;
using System;
using System.Threading.Tasks;

namespace HorseRacing.API.Controllers
{
    [ApiController]
    [Route("api/admin/tournaments")]
    [Authorize(Roles = "Admin")]
    public class AdminTournamentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminTournamentController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPut("{id}/extend")]
        public async Task<IActionResult> ExtendRegistration(long id, [FromBody] ExtendRegistrationRequest request)
        {
            if (request == null || request.AdditionalDays <= 0)
            {
                return BadRequest("Additional days must be greater than 0.");
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            if (tournament.CancelCount != 0)
            {
                return BadRequest("The tournament has already been extended once or has an invalid status.");
            }

            if (!tournament.RegistrationEndDate.HasValue || !tournament.StartDate.HasValue)
            {
                return BadRequest("Registration or start date has not been configured.");
            }

            // Tính toán ngày kết thúc đăng ký mới từ thời gian hiện tại
            DateTime baseDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            DateTime newRegistrationEndDate = baseDate.AddDays(request.AdditionalDays);

            // Validation Ràng buộc: Phải cách ngày bắt đầu giải đấu ít nhất 2 ngày trước khi đua
            if (newRegistrationEndDate > tournament.StartDate.Value.AddDays(-2))
            {
                return BadRequest("The extended registration date exceeds the limit. The new registration end date must be at least 2 days before the tournament starts.");
            }

            // Cập nhật thông tin giải đấu
            tournament.RegistrationEndDate = newRegistrationEndDate;
            tournament.CancelCount = 1;
            tournament.Status = "Registration Open"; // Mở lại trạng thái đăng ký

            await _context.SaveChangesAsync();

            // Reopening registration is relevant to every active horse owner. Existing
            // participants and their jockeys also receive a direct, actionable notice.
            await _notificationService.SendNotificationToRoleAsync(
                "HorseOwner",
                "Registration period extended",
                $"Registration for tournament '{tournament.Name}' has been extended until {newRegistrationEndDate:dd/MM/yyyy HH:mm}.",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: "/owner/tournaments");

            var participantJockeyUserIds = await _context.JockeyContracts
                .Where(c => c.TournamentId == id && (c.Status == "Accepted" || c.Status == "Active"))
                .Include(c => c.Jockey)
                .Where(c => c.Jockey != null)
                .Select(c => c.Jockey!.UserId)
                .Distinct()
                .ToListAsync();
            foreach (var userId in participantJockeyUserIds)
            {
                await _notificationService.SendNotificationToUserAsync(
                    userId,
                    "Registration period extended",
                    $"Registration for tournament '{tournament.Name}' has been extended until {newRegistrationEndDate:dd/MM/yyyy HH:mm}.",
                    "Tournament",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/jockey/schedule");
            }

            return Ok(new { Message = "Registration period extended successfully.", NewRegistrationEndDate = tournament.RegistrationEndDate });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTournament(long id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Rounds)
                    .ThenInclude(r => r.Races)
                        .ThenInclude(r => r.RaceRefereeAssignments)
                            .ThenInclude(a => a.RefereeProfile)
                .FirstOrDefaultAsync(t => t.TournamentId == id);
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            tournament.Status = "Cancelled";
            await _context.SaveChangesAsync();

            var ownerIds = await _context.Registrations
                .Where(r => r.TournamentId == id && r.Horse != null)
                .Select(r => r.Horse!.OwnerId)
                .Distinct()
                .ToListAsync();
            var jockeyIds = await _context.JockeyContracts
                .Where(c => c.TournamentId == id && c.Jockey != null)
                .Select(c => c.Jockey!.UserId)
                .Distinct()
                .ToListAsync();
            var refereeIds = tournament.Rounds.SelectMany(r => r.Races)
                .SelectMany(r => r.RaceRefereeAssignments)
                .Where(a => a.RefereeProfile != null)
                .Select(a => a.RefereeProfile!.UserId)
                .Distinct()
                .ToList();

            foreach (var userId in ownerIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", $"Tournament '{tournament.Name}' has been cancelled.", "Tournament", (int)id, actionUrl: "/owner/tournaments");
            foreach (var userId in jockeyIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", $"Tournament '{tournament.Name}' has been cancelled.", "Tournament", (int)id, actionUrl: "/jockey/schedule");
            foreach (var userId in refereeIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", $"Tournament '{tournament.Name}' has been cancelled. Your officiating assignment is no longer active.", "Tournament", (int)id, actionUrl: "/referee/schedule");
            await _notificationService.SendNotificationToRoleAsync("Spectator", "Tournament cancelled", $"Tournament '{tournament.Name}' has been cancelled.", "Tournament", (int)id, actionUrl: $"/spectator/tournaments/{id}");

            return Ok(new { Message = "Tournament cancelled successfully." });
        }
    }
}
