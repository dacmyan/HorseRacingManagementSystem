using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Spectator")]
public class SpectatorController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IBettingService _bettingService;
    private readonly IPredictionService _predictionService;

    public SpectatorController(
        IWalletService walletService,
        IBettingService bettingService,
        IPredictionService predictionService)
    {
        _walletService = walletService;
        _bettingService = bettingService;
        _predictionService = predictionService;
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

    [HttpPost("wallet/deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _walletService.DepositAsync(userId, request);
            return Ok(new { message = "Deposit successful", result = response });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during deposit", detail = ex.Message });
        }
    }

    [HttpPost("wallet/withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _walletService.WithdrawAsync(userId, request);
            return Ok(new { message = "Withdrawal successful", result = response });
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
            return StatusCode(500, new { message = "An error occurred during withdrawal", detail = ex.Message });
        }
    }

    [HttpGet("wallet/history")]
    public async Task<IActionResult> GetWalletHistory()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _walletService.GetTransactionHistoryAsync(userId);
            return Ok(new { message = "Transaction history retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving history", detail = ex.Message });
        }
    }

    [HttpGet("wallet/balance")]
    public async Task<IActionResult> GetWalletBalance()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _walletService.GetBalanceAsync(userId);
            return Ok(new { message = "Wallet balance retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving balance", detail = ex.Message });
        }
    }

    [HttpPost("bets")]
    public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _bettingService.PlaceBetAsync(userId, request);
            return Ok(new { message = "Bet placed successfully", result = response });
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
            return StatusCode(500, new { message = "An error occurred placing the bet", detail = ex.Message });
        }
    }

    [HttpGet("bets/my-bets")]
    public async Task<IActionResult> GetMyBets()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _bettingService.GetMyBetsAsync(userId);
            return Ok(new { message = "Your bets retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving bets", detail = ex.Message });
        }
    }

    [HttpPost("predictions")]
    public async Task<IActionResult> PlacePrediction([FromBody] PredictionManagementRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _predictionService.PlacePredictionAsync(userId, request);
            return Ok(new { message = "Prediction placed successfully", result = response });
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
            return StatusCode(500, new { message = "An error occurred placing prediction", detail = ex.Message });
        }
    }

    [HttpGet("predictions/stats/{raceId}")]
    public async Task<IActionResult> GetPredictionStats(int raceId)
    {
        try
        {
            var response = await _predictionService.GetPredictionStatsAsync(raceId);
            return Ok(new { message = "Prediction statistics retrieved successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving prediction stats", detail = ex.Message });
        }
    }
}
