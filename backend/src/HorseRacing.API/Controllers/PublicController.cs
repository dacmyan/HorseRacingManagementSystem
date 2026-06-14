using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
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

    [HttpGet("races/schedule")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicRaceSchedule()
    {
        try
        {
            var schedule = await _raceService.GetPublicRaceScheduleAsync();
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

        return Ok(new { message = "Round details retrieved successfully", result = round });
    }
    [HttpGet("tournaments")]
[AllowAnonymous]
public async Task<IActionResult> GetTournaments()
{
    try
    {
        var tournaments = await _tournamentService.GetAllTournamentsAsync();
        return Ok(new { message = "Tournaments retrieved successfully", result = tournaments });
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

        return Ok(new { message = "Tournament details retrieved successfully", result = tournament });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred retrieving tournament details", detail = ex.Message });
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


}
