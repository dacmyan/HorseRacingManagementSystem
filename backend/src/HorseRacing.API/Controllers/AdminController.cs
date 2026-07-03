using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IPrizePayoutService _prizePayoutService;
    private readonly IBetPayoutService _betPayoutService;
    private readonly ITournamentService _tournamentService;
    private readonly IRaceService _raceService;
    private readonly IRefereeAssignmentService _refereeAssignmentService;
    private readonly IRaceResultService _resultService;
    private readonly IRegistrationService _registrationService;

    public AdminController(
        IAdminService adminService,
        IPrizePayoutService prizePayoutService,
        IBetPayoutService betPayoutService,
        ITournamentService tournamentService,
        IRaceService raceService,
        IRefereeAssignmentService refereeAssignmentService,
        IRaceResultService resultService,
        IRegistrationService registrationService)
    {
        _adminService = adminService;
        _prizePayoutService = prizePayoutService;
        _betPayoutService = betPayoutService;
        _tournamentService = tournamentService;
        _raceService = raceService;
        _refereeAssignmentService = refereeAssignmentService;
        _resultService = resultService;
        _registrationService = registrationService;
    }

    [HttpGet("test")]
    public IActionResult TestAdminAuthorization()
    {
        return Ok(new { message = "Admin authorization successful" });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _adminService.GetRolesAsync();
        return Ok(new
        {
            message = "Roles retrieved successfully",
            result = roles
        });
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequestDto request)
    {
        try
        {
            var response = await _adminService.CreateAccountAsync(request);
            return Ok(new
            {
                message = "Account created successfully",
                result = response
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during account creation", detail = ex.Message });
        }
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        try
        {
            var accounts = await _adminService.GetAccountsAsync();
            return Ok(new
            {
                message = "Accounts retrieved successfully",
                result = accounts
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during retrieving accounts", detail = ex.Message });
        }
    }

    [HttpPost("tournaments/{id}/generate-races")]
    public async Task<IActionResult> GenerateRacesForTournament(long id)
    {
        try
        {
            var races = await _tournamentService.GenerateRacesForTournamentAsync(id);
            return Ok(new { message = "Races generated successfully", result = races });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred generating races", detail = ex.Message });
        }
    }

    [HttpPost("tournaments/{tournamentId}/generate-final")]
    public async Task<IActionResult> GenerateFinal(long tournamentId)
    {
        try
        {
            var race = await _tournamentService.GenerateFinalRaceAsync(tournamentId);
            return Ok(new { message = "Final race generated successfully", result = race });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred generating final race", detail = ex.Message });
        }
    }

    [HttpPost("races/{raceId}/recalculate-odds")]
    public async Task<IActionResult> RecalculateOdds(long raceId, [FromServices] HorseRacing.Application.Features.BettingEngine.Interfaces.IBettingService bettingService)
    {
        try
        {
            await bettingService.RecalculateRaceOddsAsync(raceId);
            return Ok(new { message = "Odds recalculated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred recalculating odds", detail = ex.Message });
        }
    }

    [HttpPost("payouts/prizes")]
    public async Task<IActionResult> DistributeTournamentPrizes([FromBody] PrizePayoutRequest request)
    {
        try
        {
            await _prizePayoutService.ProcessPrizePayoutAsync(request);
            return Ok(new { message = "Tournament prizes distributed successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during tournament prize distribution", detail = ex.Message });
        }
    }

    [HttpGet("payouts")]
    public async Task<IActionResult> GetPayouts([FromServices] AppDbContext context)
    {
        try
        {
            var payouts = await context.Payouts
                .Include(p => p.Bet)
                    .ThenInclude(b => b.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new {
                    PayoutId = p.Id,
                    BetId = p.BetId,
                    RaceId = p.Bet != null ? p.Bet.RaceId : 0,
                    SpectatorName = (p.Bet != null && p.Bet.User != null) ? p.Bet.User.FullName : "Unknown",
                    Amount = p.Amount,
                    Status = "Paid",
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(new { message = "Payouts retrieved successfully", result = payouts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving payouts", detail = ex.Message });
        }
    }

    [HttpPost("payouts/trigger/{raceId}")]
    public async Task<IActionResult> TriggerBetPayout(long raceId)
    {
        try
        {
            await _betPayoutService.ProcessPayoutAsync(raceId);
            return Ok(new { message = "Bet payouts processed successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred processing bet payouts", detail = ex.Message });
        }
    }

    [HttpPost("tournaments")]
    
    public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentRequest request)
    {
        try
        {
            var response = await _tournamentService.CreateTournamentAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during tournament creation" });
        }
    }

    [HttpPut("tournaments/{id}")]
    public async Task<IActionResult> UpdateTournament([FromRoute] long id, [FromBody] UpdateTournamentRequest request)
    {
        try
        {
            var response = await _tournamentService.UpdateTournamentAsync(id, request);
            if (response == null)
            {
                return NotFound(new { message = $"Tournament with ID {id} was not found." });
            }
            return Ok(new { message = "Tournament updated successfully", result = response });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during tournament update", detail = ex.Message });
        }
    }

    [HttpPost("tournaments/{id}/close-registration")]
    public async Task<IActionResult> CloseRegistration(long id, [FromServices] AppDbContext context)
    {
        try
        {
            var tournament = await context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound(new { message = $"Tournament with ID {id} was not found." });
            }

            tournament.RegistrationEndDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok(new { message = "Registration closed successfully.", result = new { tournamentId = id, registrationEndDate = tournament.RegistrationEndDate } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred closing registration", detail = ex.Message });
        }
    }

    [HttpPost("races")]

    public async Task<IActionResult> CreateRace([FromBody] CreateRaceRequest request)
    {
        try
        {
            var response = await _raceService.CreateRaceAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during race scheduling" });
        }
    }

    [HttpPost("races/{raceId}/entries")]
    public async Task<IActionResult> CreateRaceEntry([FromRoute] long raceId, [FromBody] CreateRaceEntryRequest request)
    {
        try
        {
            var response = await _raceService.CreateRaceEntryAsync(raceId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during race entry creation", detail = ex.Message });
        }
    }

    [HttpDelete("races/{raceId}")]
    public async Task<IActionResult> DeleteRace([FromRoute] long raceId)
    {
        try
        {
            await _raceService.DeleteRaceAsync(raceId);
            return Ok(new { message = "Race deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during race deletion", detail = ex.Message });
        }
    }

    [HttpPost("races/{raceId}/referees")]
    public async Task<IActionResult> AssignReferee([FromRoute] long raceId, [FromBody] AssignRefereeRequest request)
    {
        try
        {
            var response = await _refereeAssignmentService.AssignRefereeAsync(raceId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during referee assignment", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/referees")]
    public async Task<IActionResult> GetAssignedReferees([FromRoute] long raceId)
    {
        try
        {
            var response = await _refereeAssignmentService.GetAssignedRefereesAsync(raceId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving assigned referees", detail = ex.Message });
        }
    }

    [HttpDelete("races/{raceId}/referees/{refereeId}")]
    public async Task<IActionResult> RemoveRefereeAssignment([FromRoute] long raceId, [FromRoute] int refereeId)
    {
        try
        {
            await _refereeAssignmentService.RemoveRefereeAssignmentAsync(raceId, refereeId);
            return Ok(new { message = "Referee assignment removed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred removing referee assignment", detail = ex.Message });
        }
    }

    [HttpPost("races/{raceId}/publish")]
    public async Task<IActionResult> PublishResult([FromRoute] long raceId)
    {
        try
        {
            var response = await _resultService.PublishResultAsync(raceId);
            return Ok(new { message = "Race result published successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred publishing the race result", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/results")]
    public async Task<IActionResult> GetRaceResults([FromRoute] long raceId)
    {
        try
        {
            var response = await _resultService.GetResultsByRaceIdAsync(raceId);
            if (response == null)
            {
                return NotFound(new { message = $"Race with ID {raceId} was not found." });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving race results", detail = ex.Message });
        }
    }

    [HttpGet("registrations")]
    public async Task<IActionResult> GetRegistrations([FromServices] AppDbContext context)
    {
        try
        {
            var registrations = await context.Registrations
                .Include(r => r.Tournament)
                .Include(r => r.Horse)
                    .ThenInclude(h => h.Owner)
                .Select(r => new {
                    RegistrationId = r.RegistrationId,
                    TournamentId = r.TournamentId,
                    TournamentName = r.Tournament != null ? r.Tournament.Name : "",
                    HorseId = r.HorseId,
                    HorseName = r.Horse != null ? r.Horse.Name : "",
                    OwnerName = (r.Horse != null && r.Horse.Owner != null) ? r.Horse.Owner.FullName : "",
                    Status = r.Status,
                    RegisteredAt = r.RegisteredAt
                })
                .ToListAsync();
            return Ok(new { message = "Registrations retrieved successfully", result = registrations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving registrations", detail = ex.Message });
        }
    }

    [HttpPut("registrations/{id}/status")]
    public async Task<IActionResult> ReviewRegistration(int id, [FromBody] ReviewRegistrationRequest request, [FromServices] AppDbContext context)
    {
        try
        {
            var registration = await context.Registrations.FindAsync((long)id);
            if (registration == null)
                return NotFound(new { message = $"Registration #{id} not found." });

            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Status must be 'Approved' or 'Rejected'." });

            if (request.Status == "Approved")
            {
                if (registration.Status != "Pending")
                {
                    return BadRequest(new { message = "Only Pending registrations can be approved." });
                }

                var contract = await context.JockeyContracts.FirstOrDefaultAsync(jc => jc.TournamentId == registration.TournamentId && jc.HorseId == registration.HorseId);
                if (contract == null)
                {
                    return BadRequest(new { message = "Cannot approve registration: No jockey contract has been proposed for this horse in this tournament." });
                }

                if (contract.Status != "Accepted" && contract.Status != "Active")
                {
                    return BadRequest(new { message = $"Cannot approve registration: The jockey contract status is '{contract.Status}', but must be 'Accepted'." });
                }
            }

            registration.Status = request.Status;
            await context.SaveChangesAsync();

            return Ok(new { message = $"Registration #{id} has been {request.Status.ToLower()}.", result = new { registrationId = id, status = registration.Status } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred reviewing registration", detail = ex.Message });
        }
    }

    [HttpGet("referees")]
    public async Task<IActionResult> GetReferees([FromServices] AppDbContext context)
    {
        try
        {
            var referees = await context.RefereeProfiles
                .Include(rp => rp.User)
                .Select(rp => new
                {
                    UserId = rp.UserId,
                    RefereeId = rp.RefereeId,
                    FullName = rp.User != null ? rp.User.FullName : "",
                    Email = rp.User != null ? rp.User.Email : "",
                    LicenseNumber = rp.LicenseNumber,
                    ExperienceYears = rp.ExperienceYears,
                    Status = rp.Status
                })
                .ToListAsync();
            return Ok(new { message = "Referees retrieved successfully", result = referees });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving referees", detail = ex.Message });
        }
    }

    [HttpGet("violations")]
    public async Task<IActionResult> GetViolations([FromServices] AppDbContext context)
    {
        try
        {
            var violations = await context.Violations
                .Include(v => v.Race)
                .Select(v => new {
                    ViolationId = v.Id,
                    RaceId = v.RaceId,
                    RaceName = v.Race != null ? v.Race.Name : "",
                    Type = v.Description.Contains(":") ? v.Description.Split(':', StringSplitOptions.None)[0] : "Vi phạm",
                    Note = v.Description,
                    Penalty = v.Penalty,
                    Status = v.Status,
                    CreatedAt = DateTime.UtcNow
                })
                .ToListAsync();
            return Ok(new { message = "Violations retrieved successfully", result = violations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving violations", detail = ex.Message });
        }
    }

    [HttpGet("predictions/stats")]
    public async Task<IActionResult> GetPredictionStats([FromServices] AppDbContext context)
    {
        try
        {
            var predictions = await context.Predictions.ToListAsync();
            var total = predictions.Count;
            var correct = predictions.Count(p => p.IsCorrect == true);
            var wrong = predictions.Count(p => p.IsCorrect == false);
            var accuracyRate = total > 0 ? (double)correct * 100 / total : 0;

            var stats = new {
                TotalPredictions = total,
                CorrectPredictions = correct,
                WrongPredictions = wrong,
                AccuracyRate = accuracyRate
            };

            return Ok(new { message = "Prediction stats retrieved successfully", result = stats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving stats", detail = ex.Message });
        }
    }

    [HttpGet("predictions")]
    public async Task<IActionResult> GetPredictions([FromServices] AppDbContext context)
    {
        try
        {
            var predictions = await context.Predictions
                .Include(p => p.User)
                .Include(p => p.Race)
                .Include(p => p.RaceEntry)
                    .ThenInclude(re => re.Registration)
                        .ThenInclude(reg => reg.Horse)
                .Select(p => new {
                    PredictionId = p.PredictionId,
                    SpectatorName = p.User != null ? p.User.FullName : "Unknown",
                    RaceName = p.Race != null ? p.Race.Name : "Unknown Race",
                    PredictedWinner = (p.RaceEntry != null && p.RaceEntry.Registration != null && p.RaceEntry.Registration.Horse != null) ? p.RaceEntry.Registration.Horse.Name : "Unknown Horse",
                    Point = p.Point,
                    IsCorrect = p.IsCorrect,
                    Status = p.Status,
                    PredictedAt = p.PredictedAt
                })
                .ToListAsync();

            return Ok(new { message = "Predictions retrieved successfully", result = predictions });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving predictions", detail = ex.Message });
        }
    }

    [HttpPut("registrations/{id}/approve")]
    public async Task<IActionResult> ApproveRegistration([FromRoute] long id)
    {
        try
        {
            var request = new ReviewRegistrationRequest { Status = "Approved" };
            var response = await _registrationService.ReviewRegistrationAsync(id, request);
            return Ok(new { message = "Registration approved successfully", result = response });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred approving registration", detail = ex.Message });
        }
    }

    [HttpPut("registrations/{id}/reject")]
    public async Task<IActionResult> RejectRegistration([FromRoute] long id)
    {
        try
        {
            var request = new ReviewRegistrationRequest { Status = "Rejected" };
            var response = await _registrationService.ReviewRegistrationAsync(id, request);
            return Ok(new { message = "Registration rejected successfully", result = response });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred rejecting registration", detail = ex.Message });
        }
    }

    [HttpGet("activity-log")]
    public async Task<IActionResult> GetActivityLog([FromServices] AppDbContext context)
    {
        try
        {
            var activities = new List<object>();

            // Recent users
            var recentUsers = await context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new { Type = "User", Title = "New user registered", Description = u.FullName + " (" + u.Email + ")", CreatedAt = u.CreatedAt })
                .ToListAsync();
            activities.AddRange(recentUsers);

            // Recent registrations
            var recentRegistrations = await context.Registrations
                .Include(r => r.Horse)
                .Include(r => r.Tournament)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(10)
                .Select(r => new { Type = "Registration", Title = "Horse registration " + r.Status, Description = (r.Horse != null ? r.Horse.Name : "") + " registered for " + (r.Tournament != null ? r.Tournament.Name : ""), CreatedAt = r.RegisteredAt })
                .ToListAsync();
            activities.AddRange(recentRegistrations);

            // Recent bets
            var recentBets = await context.Bets
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new { Type = "Bet", Title = "Bet placed", Description = (b.User != null ? b.User.FullName : "Unknown") + " bet " + b.Amount + " on race " + b.RaceId, CreatedAt = b.CreatedAt })
                .ToListAsync();
            activities.AddRange(recentBets);

            // Recent notifications
            var recentNotifications = await context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new { Type = "Notification", Title = "System notification", Description = n.Message, CreatedAt = n.CreatedAt })
                .ToListAsync();
            activities.AddRange(recentNotifications);

            // Recent wallet transactions
            var recentTransactions = await context.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new { Type = "Transaction", Title = "Wallet " + t.Type, Description = "Amount: " + t.Amount, CreatedAt = t.CreatedAt })
                .ToListAsync();
            activities.AddRange(recentTransactions);

            // Sort all by CreatedAt descending and take top 50
            var sorted = activities
                .OrderByDescending(a => ((dynamic)a).CreatedAt)
                .Take(50)
                .ToList();

            return Ok(new { message = "Activity log retrieved successfully", result = sorted });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving activity log", detail = ex.Message });
        }
    }

    [HttpGet("users/options")]
    public async Task<IActionResult> GetUserOptions([FromServices] AppDbContext context)
    {
        try
        {
            var users = await context.Users
                .Include(u => u.Role)
                .Where(u => u.Status == "Active")
                .Select(u => new
                {
                    Id = u.UserId,
                    Label = u.FullName,
                    Extra = u.Role != null ? u.Role.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(new { message = "User options retrieved successfully", result = users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving user options", detail = ex.Message });
        }
    }

    [HttpGet("horses/options")]
    public async Task<IActionResult> GetHorseOptions([FromServices] AppDbContext context)
    {
        try
        {
            var horses = await context.Horses
                .Include(h => h.Owner)
                .Select(h => new
                {
                    Id = (int)h.HorseId,
                    Label = h.Name,
                    Extra = "Owner: " + (h.Owner != null ? h.Owner.FullName : "Unknown")
                })
                .ToListAsync();

            return Ok(new { message = "Horse options retrieved successfully", result = horses });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving horse options", detail = ex.Message });
        }
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromServices] AppDbContext context)
    {
        try
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            user.Status = user.Status == "Active" ? "Inactive" : "Active";
            await context.SaveChangesAsync();

            return Ok(new { message = "User status updated successfully", result = user });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating user status", detail = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats([FromServices] AppDbContext context)
    {
        try
        {
            var totalUsers = await context.Users.CountAsync();
            var totalTournaments = await context.Tournaments.CountAsync();
            var activeRaces = await context.Races.CountAsync(r => r.Status == "Live" || r.Status == "Scheduled");
            var totalBets = await context.Bets.CountAsync();
            
            var totalRevenue = await context.Bets.Where(b => b.Status != "Pending").SumAsync(b => (decimal?)b.Amount) ?? 0;
            var totalPayout = await context.Payouts.SumAsync(p => (decimal?)p.Amount) ?? 0;

            var result = new
            {
                TotalUsers = totalUsers,
                TotalTournaments = totalTournaments,
                ActiveRaces = activeRaces,
                TotalBets = totalBets,
                TotalRevenue = totalRevenue,
                TotalPayout = totalPayout,
                Profit = totalRevenue - totalPayout
            };

            return Ok(new { message = "Dashboard stats retrieved successfully", result = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving dashboard stats", detail = ex.Message });
        }
    }

    [HttpPut("violations/{id}/status")]
    public async Task<IActionResult> UpdateViolationStatus(int id, [FromBody] UpdateViolationStatusRequest request, [FromServices] AppDbContext context)
    {
        try
        {
            var violation = await context.Violations.FindAsync(id);
            if (violation == null)
            {
                return NotFound(new { message = $"Violation with ID {id} was not found." });
            }

            if (request.Status != "Pending" && request.Status != "Confirmed" && request.Status != "Rejected")
            {
                return BadRequest(new { message = "Invalid status. Must be 'Pending', 'Confirmed', or 'Rejected'." });
            }

            violation.Status = request.Status;
            await context.SaveChangesAsync();

            return Ok(new { message = "Violation status updated successfully", result = violation });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating violation status", detail = ex.Message });
        }
    }

    [HttpGet("races/referee-assignments")]
    public async Task<IActionResult> GetRacesRefereeAssignments([FromServices] AppDbContext context)
    {
        try
        {
            var races = await context.Races
                .Include(r => r.Round)
                    .ThenInclude(rd => rd.Tournament)
                .Include(r => r.RaceRefereeAssignments)
                    .ThenInclude(ra => ra.RefereeProfile)
                        .ThenInclude(rp => rp.User)
                .Select(r => new
                {
                    RaceId = r.RaceId,
                    RaceName = r.Name,
                    RaceDate = r.RaceDate,
                    Status = r.Status,
                    DistanceMeter = r.DistanceMeter,
                    RoundName = r.Round != null ? r.Round.Name : "",
                    TournamentName = (r.Round != null && r.Round.Tournament != null) ? r.Round.Tournament.Name : "",
                    Referees = r.RaceRefereeAssignments.Select(ra => new
                    {
                        RefereeId = ra.RefereeId,
                        FullName = (ra.RefereeProfile != null && ra.RefereeProfile.User != null) ? ra.RefereeProfile.User.FullName : "",
                        LicenseNumber = ra.RefereeProfile != null ? ra.RefereeProfile.LicenseNumber : "",
                        Status = ra.Status
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { message = "Races and referee assignments retrieved successfully", result = races });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving races and referee assignments", detail = ex.Message });
        }
    }
}

public class UpdateViolationStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
