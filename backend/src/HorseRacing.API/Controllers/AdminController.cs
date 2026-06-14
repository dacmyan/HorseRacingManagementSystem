using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public AdminController(
        IAdminService adminService,
        IPrizePayoutService prizePayoutService,
        IBetPayoutService betPayoutService,
        ITournamentService tournamentService,
        IRaceService raceService,
        IRefereeAssignmentService refereeAssignmentService,
        IRaceResultService resultService)
    {
        _adminService = adminService;
        _prizePayoutService = prizePayoutService;
        _betPayoutService = betPayoutService;
        _tournamentService = tournamentService;
        _raceService = raceService;
        _refereeAssignmentService = refereeAssignmentService;
        _resultService = resultService;
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
}