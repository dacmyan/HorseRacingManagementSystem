using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.Notifications.DTOs;
using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.HorseManagement.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

private readonly AppDbContext _context;
private readonly INotificationService _notificationService;
private readonly IRaceService _raceService;
private readonly IRoundService _roundService;
private readonly ITournamentService _tournamentService;
private readonly IRaceResultService _resultService;


    public PublicController(
    AppDbContext context,
    INotificationService notificationService,
    IRaceService raceService,
    IRoundService roundService,
    ITournamentService tournamentService,
    IRaceResultService resultService)
{
    _context = context;
    _notificationService = notificationService;
    _raceService = raceService;
    _roundService = roundService;
    _tournamentService = tournamentService;
    _resultService = resultService;
}

    private int GetCurrentUserId()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier))
        {
            nameIdentifier = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        return int.Parse(nameIdentifier ?? "0");
    }

    [HttpGet("rankings/jockeys")]
    [AllowAnonymous]
    public async Task<IActionResult> GetJockeyRankings()
    {
        try
        {
            var rankings = await _context.JockeyProfiles
                .Include(jp => jp.User)
                .Where(jp => jp.Status == "Active")
                .OrderByDescending(jp => jp.RankingPoint)
                .Select(jp => new JockeyRankingResponse
                {
                    JockeyId = (int)jp.JockeyId,
                    UserId = jp.UserId,
                    FullName = jp.User != null ? jp.User.FullName : "Unknown Jockey",
                    Email = jp.User != null ? jp.User.Email : string.Empty,
                    ExperienceYears = jp.ExperienceYears,
                    RankingPoint = jp.RankingPoint
                })
                .ToListAsync();

            return Ok(new { message = "Jockey rankings retrieved successfully", result = rankings });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving jockey rankings", detail = ex.Message });
        }
    }

    [HttpGet("rankings/horses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHorseRankings()
    {
        try
        {
            // Fetch all finished results
            var results = await _context.RaceResults.ToListAsync();
            
            // Fetch all horses with owners
            var horses = await _context.Horses
                .Include(h => h.Owner)
                .ToListAsync();

            var rankings = horses
                .Select(h =>
                {
                    // Calculate wins count based on name match or ID match in Winner column
                    var wins = results.Count(r => 
                        r.Winner.Equals(h.Name, StringComparison.OrdinalIgnoreCase) || 
                        r.Winner.Equals(h.HorseId.ToString()));

                    return new HorseRankingResponse
                    {
                        HorseId = (int)h.HorseId,
                        Name = h.Name,
                        Age = h.Age,
                        Breed = h.Breed,
                        OwnerName = h.Owner != null ? h.Owner.FullName : "Unknown Owner",
                        WinsCount = wins
                    };
                })
                .OrderByDescending(h => h.WinsCount)
                .ToList();

            return Ok(new { message = "Horse rankings retrieved successfully", result = rankings });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving horse rankings", detail = ex.Message });
        }
    }

    [HttpGet("notifications")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] string? type,
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetNotificationsForUserPagedAsync(userId, type, isRead, page, pageSize);
            return Ok(new { message = "Notifications retrieved successfully", result = notifications });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving notifications", detail = ex.Message });
        }
    }

    [HttpPut("notifications/{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(new { message = "Notification marked as read successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating notification", detail = ex.Message });
        }
    }

    [HttpPut("notifications/read-all")]
    [Authorize]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating notifications", detail = ex.Message });
        }
    }

    [HttpDelete("notifications/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
            return Ok(new { message = "Notification soft deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred deleting notification", detail = ex.Message });
        }
    }

    [HttpGet("races/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicRaceSchedule()
    {
        try
        {
            var schedule = await _raceService.GetPublicRaceScheduleAsync();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                var now = VietnamNow;
                var futureTournamentIds = await _context.Tournaments
                    .Where(t => 
                        (!t.RegistrationStartDate.HasValue || t.RegistrationStartDate.Value > now) && 
                        (!t.StartDate.HasValue || t.StartDate.Value > now)
                    )
                    .Select(t => t.TournamentId)
                    .ToListAsync();

                if (futureTournamentIds.Any())
                {
                    schedule = schedule.Where(s => !futureTournamentIds.Contains(s.TournamentId)).ToList();
                }
            }

            return Ok(new { message = "Public race schedule retrieved successfully", result = schedule });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred retrieving public race schedule" });
        }
    }

    [HttpGet("tournaments/{tournamentId}/rounds")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoundsByTournament(long tournamentId)
    {
        var tournament = await _tournamentService.GetTournamentByIdAsync(tournamentId);
        if (tournament == null)
        {
            return NotFound(new { message = $"Tournament with ID {tournamentId} was not found." });
        }

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && 
            (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
            (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
        {
            return NotFound(new { message = $"Tournament with ID {tournamentId} was not found." });
        }

        var rounds = await _roundService.GetRoundsByTournamentIdAsync(tournamentId);
        if (rounds == null)
        {
            return NotFound(new { message = $"Tournament with ID {tournamentId} was not found." });
        }

        return Ok(new { message = "Rounds retrieved successfully", result = rounds });
    }

    [HttpGet("rounds/{roundId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoundDetail(long roundId)
    {
        var round = await _roundService.GetRoundByIdAsync(roundId);
        if (round == null)
        {
            return NotFound(new { message = $"Round with ID {roundId} was not found." });
        }

        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            var tournament = await _context.Tournaments.FindAsync(round.TournamentId);
            if (tournament != null && 
                (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
                (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
            {
                return NotFound(new { message = $"Round with ID {roundId} was not found." });
            }
        }

        return Ok(new { message = "Round details retrieved successfully", result = round });
    }

    [HttpGet("tournaments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTournaments()
    {
        try
        {
            var tournaments = await _tournamentService.GetAllTournamentsAsync();
            
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAllowed = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "Referee", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "HorseOwner", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "Spectator", StringComparison.OrdinalIgnoreCase);

            if (!isAllowed)
            {
                var now = VietnamNow;
                tournaments = tournaments.Where(t => 
                    (t.RegistrationStartDate.HasValue && t.RegistrationStartDate.Value <= now) || 
                    (t.StartDate.HasValue && t.StartDate.Value <= now) ||
                    (!t.RegistrationStartDate.HasValue && !t.StartDate.HasValue)
                ).ToList();
            }

            var tournamentIds = tournaments.Select(t => t.TournamentId).ToList();
            var prizes = await _context.Prizes
                .Where(p => tournamentIds.Contains(p.TournamentId))
                .ToListAsync();

            var prizesGrouped = prizes.GroupBy(p => p.TournamentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var registrations = await _context.Registrations
                .Include(r => r.MedicalCheckRecords)
                .Where(r => tournamentIds.Contains(r.TournamentId))
                .ToListAsync();

            var registrationsGrouped = registrations.GroupBy(r => r.TournamentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = tournaments.Select(t => {
                var tournamentRegs = registrationsGrouped.ContainsKey(t.TournamentId)
                    ? registrationsGrouped[t.TournamentId]
                    : new List<Registration>();

                var approvedRegistration = tournamentRegs.Count(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase));
                var qualifiedRegistration = tournamentRegs.Count(r => 
                {
                    if (!string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase)) return false;
                    var check = r.MedicalCheckRecords?.FirstOrDefault();
                    if (check == null) return false;
                    bool isMedicalPassed = string.Equals(check.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) || 
                                           string.Equals(check.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase);
                    bool isDopingNegative = !string.Equals(check.DopingResult, "Positive", StringComparison.OrdinalIgnoreCase);
                    return isMedicalPassed && isDopingNegative;
                });

                return new {
                    t.TournamentId,
                    t.Name,
                    t.Description,
                    t.RegistrationStartDate,
                    t.RegistrationEndDate,
                    t.StartDate,
                    t.EndDate,
                    t.Status,
                    t.Rounds,
                    t.CancelCount,
                    t.HasMissingReferees,
                    t.HasCompleteLaneAssignments,
                    ApprovedRegistration = approvedRegistration,
                    QualifiedRegistration = qualifiedRegistration,
                    Prizes = prizesGrouped.ContainsKey(t.TournamentId)
                        ? prizesGrouped[t.TournamentId].Select(p => (object)new { p.Id, p.RankPosition, p.Amount }).ToList()
                        : new List<object>()
                };
            }).ToList();

            return Ok(new { message = "Tournaments retrieved successfully", result = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving tournaments", detail = ex.Message });
        }
    }

    [HttpGet("tournaments/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTournamentDetail(long id)
    {
        try
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound(new { message = $"Tournament with ID {id} was not found." });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAllowed = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "Referee", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "HorseOwner", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(role, "Spectator", StringComparison.OrdinalIgnoreCase);

            if (!isAllowed && 
                (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
                (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
            {
                return NotFound(new { message = $"Tournament with ID {id} was not found." });
            }

            var prizes = await _context.Prizes
                .Where(p => p.TournamentId == id)
                .Select(p => new { p.Id, p.RankPosition, p.Amount })
                .ToListAsync();

            var registrations = await _context.Registrations
                .Include(r => r.MedicalCheckRecords)
                .Where(r => r.TournamentId == id)
                .ToListAsync();

            var approvedRegistration = registrations.Count(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase));
            var qualifiedRegistration = registrations.Count(r => 
            {
                if (!string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase)) return false;
                var check = r.MedicalCheckRecords?.FirstOrDefault();
                if (check == null) return false;
                bool isMedicalPassed = string.Equals(check.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) || 
                                       string.Equals(check.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase);
                bool isDopingNegative = !string.Equals(check.DopingResult, "Positive", StringComparison.OrdinalIgnoreCase);
                return isMedicalPassed && isDopingNegative;
            });

            var result = new {
                tournament.TournamentId,
                tournament.Name,
                tournament.Description,
                tournament.RegistrationStartDate,
                tournament.RegistrationEndDate,
                tournament.StartDate,
                tournament.EndDate,
                tournament.Status,
                tournament.Rounds,
                tournament.CancelCount,
                tournament.HasMissingReferees,
                ApprovedRegistration = approvedRegistration,
                QualifiedRegistration = qualifiedRegistration,
                Prizes = prizes
            };

            return Ok(new { message = "Tournament details retrieved successfully", result = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving tournament details", detail = ex.Message });
        }
    }

    [HttpGet("tournaments/{id}/qualified-horses")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQualifiedHorses(long id)
    {
        try
        {
            var result = await _tournamentService.GetQualifiedHorsesAsync(id);
            return Ok(new { message = "Qualified horses retrieved successfully", result = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving qualified horses", detail = ex.Message });
        }
    }

    [HttpGet("races/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRaceDetail(long id)
    {
        try
        {
            var race = await _raceService.GetRaceByIdAsync(id);
            if (race == null)
            {
                return NotFound(new { message = $"Race with ID {id} was not found." });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                var tournament = await _context.Tournaments.FindAsync(race.TournamentId);
                if (tournament != null && 
                    (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
                    (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
                {
                    return NotFound(new { message = $"Race with ID {id} was not found." });
                }
            }

            return Ok(new { message = "Race details retrieved successfully", result = race });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving race details", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/entries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRaceEntries(long raceId)
    {
        try
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                var race = await _context.Races
                    .Include(r => r.Round)
                    .FirstOrDefaultAsync(r => r.RaceId == raceId);
                if (race != null && race.Round != null)
                {
                    var tournament = await _context.Tournaments.FindAsync(race.Round.TournamentId);
                    if (tournament != null && 
                        (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
                        (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
                    {
                        return NotFound(new { message = $"Race with ID {raceId} was not found." });
                    }
                }
            }

            var entries = await _raceService.GetRaceEntriesByRaceIdAsync(raceId);
            if (entries == null)
            {
                return NotFound(new { message = $"Race with ID {raceId} was not found." });
            }

            return Ok(new { message = "Race entries retrieved successfully", result = entries });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving race entries", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/results")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRaceResults(long raceId)
    {
        try
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                var race = await _context.Races
                    .Include(r => r.Round)
                    .FirstOrDefaultAsync(r => r.RaceId == raceId);
                if (race != null && race.Round != null)
                {
                    var tournament = await _context.Tournaments.FindAsync(race.Round.TournamentId);
                    if (tournament != null && 
                        (!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) && 
                        (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
                    {
                        return NotFound(new { message = $"Race with ID {raceId} was not found." });
                    }
                }
            }

            var response = await _resultService.GetPublicResultsByRaceIdAsync(raceId);
            if (response == null)
            {
                return NotFound(new { message = $"Race with ID {raceId} was not found." });
            }

            return Ok(new { message = "Race results retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving race results", detail = ex.Message });
        }
    }

[HttpGet("races/live")]
[AllowAnonymous]
public async Task<IActionResult> GetLiveRaces()
{
    try
    {
        var liveStatuses = new[] { "Live", "Running", "InProgress", "Ongoing" };
        var liveRaces = await _context.Races
            .Include(r => r.Round)
                .ThenInclude(rd => rd.Tournament)
            .Where(r => liveStatuses.Contains(r.Status))
            .Select(r => new
            {
                RaceId = r.RaceId,
                RaceName = r.Name,
                TournamentName = r.Round != null && r.Round.Tournament != null ? r.Round.Tournament.Name : "",
                StartTime = r.RaceDate,
                Status = r.Status
            })
            .ToListAsync();

        return Ok(new { message = "Live races retrieved successfully", result = liveRaces });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred retrieving live races", detail = ex.Message });
    }
}

    [HttpPost("tournaments/{id}/generate-races")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateRacesForTournament(long id)
    {
        try
        {
            var races = await _tournamentService.GenerateRacesForTournamentAsync(id);
            return Ok(new { message = "Races generated successfully", result = races });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }
}
