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

    public AdminController(
        IAdminService adminService,
        IPrizePayoutService prizePayoutService,
        IBetPayoutService betPayoutService,
        ITournamentService tournamentService,
        IRaceService raceService,
        IRefereeAssignmentService refereeAssignmentService)
    {
        _adminService = adminService;
        _prizePayoutService = prizePayoutService;
        _betPayoutService = betPayoutService;
        _tournamentService = tournamentService;
        _raceService = raceService;
        _refereeAssignmentService = refereeAssignmentService;
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
    public async Task<IActionResult> RemoveRefereeAssignment([FromRoute] long raceId, [FromRoute] long refereeId)
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
}