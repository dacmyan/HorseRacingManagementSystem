using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HorseRacing.Infrastructure.Persistence;
using HorseRacing.Application.Features.Notifications.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
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
        public async Task<IActionResult> ExtendRegistration(long id)
        {
            if (id <= 0)
                return BadRequest("Tournament ID must be greater than zero.");

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            if (tournament.CancelCount != 0)
            {
                return BadRequest("The tournament has already been extended once or has an invalid status.");
            }
            var extendableStatuses = new[] { "PendingRegistration", "Registration Open", "PendingScheduling" };
            if (!extendableStatuses.Contains(tournament.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest($"Tournament in status '{tournament.Status}' cannot be extended.");

            if (!tournament.RegistrationEndDate.HasValue || !tournament.StartDate.HasValue)
            {
                return BadRequest("Registration or start date has not been configured.");
            }

            // Tính toán ngày kết thúc đăng ký mới từ thời gian hiện tại
            DateTime baseDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            if (baseDate < tournament.RegistrationEndDate.Value)
            {
                return BadRequest("Registration can only be extended after the original registration period has ended.");
            }

            var registrations = await _context.Registrations
                .Include(r => r.MedicalCheckRecords)
                .Where(r => r.TournamentId == id && r.Status == "Approved")
                .ToListAsync();
            var qualifiedCount = registrations.Count(r => r.MedicalCheckRecords.Any(check =>
                (check.MedicalResult == "Pass" || check.MedicalResult == "Passed") &&
                check.DopingResult != "Positive"));
            if (qualifiedCount >= 12)
            {
                return BadRequest($"This tournament already has {qualifiedCount} qualified horses and does not need an extension.");
            }

            DateTime newRegistrationEndDate = tournament.StartDate.Value.AddHours(-48);

            // Validation Ràng buộc: Phải cách ngày bắt đầu giải đấu ít nhất 2 ngày trước khi đua
            if (newRegistrationEndDate <= baseDate)
            {
                return BadRequest("Registration cannot be extended because the final 48-hour preparation window has already started. Please cancel the tournament.");
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

            return Ok(new { Message = "Registration extended once and will close 48 hours before the tournament starts.", NewRegistrationEndDate = tournament.RegistrationEndDate, QualifiedHorses = qualifiedCount });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTournament(long id, [FromBody] CancelTournamentRequest request)
        {
            if (id <= 0)
                return BadRequest("Tournament ID must be greater than zero.");

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

            if (new[] { "Active", "AwaitingResults", "Completed", "Cancelled" }
                .Contains(tournament.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest($"Tournament in status '{tournament.Status}' cannot be cancelled.");

            var raceIds = await _context.Rounds.Where(r => r.TournamentId == id)
                .SelectMany(r => r.Races).Select(r => r.RaceId).ToListAsync();
            if (await _context.Bets.AnyAsync(b => raceIds.Contains(b.RaceId)))
                return BadRequest("Tournament with existing bets cannot be cancelled until bets are refunded.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            tournament.Status = "Cancelled";
            var registrations = await _context.Registrations.Where(r => r.TournamentId == id).ToListAsync();
            foreach (var registration in registrations.Where(r => r.Status is "Pending" or "Approved"))
                registration.Status = "Cancelled";
            var races = await _context.Races.Where(r => raceIds.Contains(r.RaceId)).ToListAsync();
            foreach (var race in races)
                race.Status = "Cancelled";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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

            var cancellationMessage = $"Tournament '{tournament.Name}' has been cancelled. Reason: {request.Reason.Trim()}";
            foreach (var userId in ownerIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", cancellationMessage, "Tournament", (int)id, actionUrl: "/owner/tournaments");
            foreach (var userId in jockeyIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", cancellationMessage, "Tournament", (int)id, actionUrl: "/jockey/schedule");
            foreach (var userId in refereeIds)
                await _notificationService.SendNotificationToUserAsync(userId, "Tournament cancelled", $"{cancellationMessage} Your officiating assignment is no longer active.", "Tournament", (int)id, actionUrl: "/referee/schedule");
            await _notificationService.SendNotificationToRoleAsync("Spectator", "Tournament cancelled", cancellationMessage, "Tournament", (int)id, actionUrl: $"/spectator/tournaments/{id}");

            return Ok(new { Message = "Tournament cancelled successfully." });
        }
    }

    public sealed class CancelTournamentRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(500, MinimumLength = 5)]
        public string Reason { get; set; } = string.Empty;
    }
}
