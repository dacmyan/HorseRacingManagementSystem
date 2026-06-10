using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

    public AdminController(
        IAdminService adminService,
        IPrizePayoutService prizePayoutService,
        IBetPayoutService betPayoutService)
    {
        _adminService = adminService;
        _prizePayoutService = prizePayoutService;
        _betPayoutService = betPayoutService;
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
    public async Task<IActionResult> TriggerBetPayout(int raceId)
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
}