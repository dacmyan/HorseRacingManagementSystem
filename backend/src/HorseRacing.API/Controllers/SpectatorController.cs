using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacing.API.Filters;

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
    [BlockLockedUser]
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

    [HttpGet("races/{raceId}/betting-info")]
    public async Task<IActionResult> GetRaceBettingInfo([FromRoute] long raceId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _bettingService.GetRaceBettingInfoAsync(userId, raceId);
            return Ok(new { message = "Race betting info retrieved successfully", result = response });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving race betting info", detail = ex.Message });
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
    [BlockLockedUser]
    public async Task<IActionResult> CreatePrediction([FromBody] CreatePredictionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _predictionService.CreatePredictionAsync(userId, request);
            return Ok(new { message = "Prediction submitted successfully", result = response });
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
            return StatusCode(500, new { message = "An error occurred submitting the prediction", detail = ex.Message });
        }
    }

    [HttpGet("predictions/my-predictions")]
    public async Task<IActionResult> GetMyPredictions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _predictionService.GetMyPredictionsAsync(userId);
            return Ok(new { message = "Your predictions retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving predictions", detail = ex.Message });
        }
    }

    [HttpGet("predictions/race/{raceId}")]
    public async Task<IActionResult> GetPredictionsByRace(long raceId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var predictions = await _predictionService.GetMyPredictionsAsync(userId);
            var prediction = predictions.FirstOrDefault(p => p.RaceId == raceId);
            if (prediction == null)
            {
                return NotFound(new { message = $"No prediction found for race ID {raceId}." });
            }
            return Ok(new { message = "Prediction retrieved successfully", result = prediction });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving prediction for the race", detail = ex.Message });
        }
    }
}
