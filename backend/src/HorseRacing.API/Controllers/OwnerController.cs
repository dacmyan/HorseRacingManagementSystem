using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.HorseManagement.DTOs;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "HorseOwner")]
public class OwnerController : ControllerBase
{
    private readonly IHorseService _horseService;
    private readonly IHorseDocumentService _horseDocumentService;
    private readonly IJockeyContractService _jockeyContractService;
    private readonly IRegistrationService _registrationService;
    private readonly IWalletService _walletService;

    public OwnerController(
        IHorseService horseService,
        IHorseDocumentService horseDocumentService,
        IJockeyContractService jockeyContractService,
        IRegistrationService registrationService,
        IWalletService walletService)
    {
        _horseService = horseService;
        _horseDocumentService = horseDocumentService;
        _jockeyContractService = jockeyContractService;
        _registrationService = registrationService;
        _walletService = walletService;
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

    [HttpPost("horses")]
    public async Task<IActionResult> CreateHorse([FromBody] RegisterHorseRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _horseService.CreateHorseAsync(userId, request);
            return CreatedAtAction(nameof(GetHorseById), new { id = response.Id }, new { message = "Horse registered successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during horse creation", detail = ex.Message });
        }
    }

    [HttpGet("horses/my-horses")]
    public async Task<IActionResult> GetMyHorses()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _horseService.GetHorsesByOwnerAsync(userId);
            return Ok(new { message = "Horses retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving your horses", detail = ex.Message });
        }
    }

    [HttpGet("horses/{id}")]
    public async Task<IActionResult> GetHorseById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _horseService.GetHorseByIdAsync(id, userId);
            if (response == null)
            {
                return NotFound(new { message = $"Horse with ID {id} not found or access denied." });
            }
            return Ok(new { message = "Horse details retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving horse details", detail = ex.Message });
        }
    }

    [HttpPut("horses/{id}")]
    public async Task<IActionResult> UpdateHorse(int id, [FromBody] UpdateHorseRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _horseService.UpdateHorseAsync(id, userId, request);
            return Ok(new { message = "Horse updated successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Forbid(); // 403 Forbidden
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating horse details", detail = ex.Message });
        }
    }

    [HttpDelete("horses/{id}")]
    public async Task<IActionResult> DeleteHorse(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _horseService.DeleteHorseAsync(id, userId);
            return Ok(new { message = "Horse deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred deleting the horse", detail = ex.Message });
        }
    }

    [HttpPost("horses/{id}/documents")]
    public async Task<IActionResult> UploadDocument(int id, [FromBody] UploadHorseDocumentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _horseDocumentService.AddDocumentAsync(userId, id, request);
            return Ok(new { message = "Document uploaded successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred uploading the document", detail = ex.Message });
        }
    }

    [HttpPost("jockey-contracts")]
    public async Task<IActionResult> CreateContract([FromBody] CreateJockeyContract request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _jockeyContractService.SendContractAsync(userId, request);
            return Ok(new { message = "Jockey contract proposed successfully", result = response });
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
            return StatusCode(500, new { message = "An error occurred sending the contract", detail = ex.Message });
        }
    }

    [HttpGet("jockey-contracts/my-proposals")]
    public async Task<IActionResult> GetMyProposedContracts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _jockeyContractService.GetContractsForOwnerAsync(userId);
            return Ok(new { message = "Proposed contracts retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving proposed contracts", detail = ex.Message });
        }
    }

    [HttpGet("jockeys/{jockeyId:int}/check-busy/{tournamentId:long}")]
    public async Task<IActionResult> CheckJockeyBusy(int jockeyId, long tournamentId, [FromServices] AppDbContext context)
    {
        try
        {
            var isBusy = await context.JockeyContracts
                .AnyAsync(jc => jc.JockeyId == jockeyId 
                    && jc.TournamentId == tournamentId 
                    && (jc.Status == "Active" || jc.Status == "Accepted"));
                    
            return Ok(new { isBusy });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error checking jockey status", detail = ex.Message });
        }
    }

    [HttpGet("horses/{horseId:int}/check-busy/{tournamentId:long}")]
    public async Task<IActionResult> CheckHorseBusy(int horseId, long tournamentId, [FromServices] AppDbContext context)
    {
        try
        {
            var isBusy = await context.JockeyContracts
                .AnyAsync(jc => jc.HorseId == horseId 
                    && jc.TournamentId == tournamentId 
                    && (jc.Status == "Pending" || jc.Status == "Active" || jc.Status == "Accepted"));
                    
            return Ok(new { isBusy });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error checking horse status", detail = ex.Message });
        }
    }

    [HttpDelete("jockey-contracts/{id:int}")]
    public async Task<IActionResult> CancelContract(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _jockeyContractService.CancelContractAsync(userId, id);
            return Ok(new { message = "Jockey contract invitation cancelled successfully", result = response });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred cancelling the contract", detail = ex.Message });
        }
    }

    [HttpPost("registrations")]
    public async Task<IActionResult> RegisterHorse([FromBody] CreateRegistrationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _registrationService.RegisterHorseAsync(userId, request);
            return Ok(new { message = "Tournament registration submitted successfully", result = response });
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
            return StatusCode(500, new { message = "An error occurred submitting registration", detail = ex.Message });
        }
    }

    [HttpGet("registrations/my-registrations")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _registrationService.GetRegistrationsByOwnerAsync(userId);
            return Ok(new { message = "Your registrations retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving registrations", detail = ex.Message });
        }
    }

    [HttpGet("owner/results")]
    public async Task<IActionResult> GetOwnerResults([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            // Fetch owner's horses
            var horseIds = await context.Horses
                .Where(h => h.OwnerId == userId)
                .Select(h => h.HorseId)
                .ToListAsync();

            if (!horseIds.Any())
            {
                return Ok(new { message = "Results retrieved successfully", result = new List<object>() });
            }

            // Fetch race entries for these horses
            var results = await context.RaceEntries
                .Include(re => re.Race)
                    .ThenInclude(r => r.Round)
                        .ThenInclude(r0 => r0.Tournament)
                .Include(re => re.Registration)
                    .ThenInclude(reg => reg.Horse)
                .Where(re => horseIds.Contains(re.Registration.HorseId))
                .OrderByDescending(re => re.Race != null ? re.Race.RaceDate : DateTime.MinValue)
                .ToListAsync();

            // Fetch the winners for these races
            var raceIds = results.Select(re => re.RaceId).Distinct().ToList();
            var winners = await context.RaceResults
                .Where(rr => raceIds.Contains(rr.RaceId))
                .ToDictionaryAsync(rr => rr.RaceId, rr => rr.Winner);

            // Fetch tournament prizes
            var tournamentIds = results.Select(re => re.Race?.Round?.TournamentId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var prizes = await context.Prizes
                .Where(p => tournamentIds.Contains(p.TournamentId))
                .ToListAsync();

            var ownerResults = results.Select(re => {
                var horseName = re.Registration?.Horse?.Name ?? "";
                var horseIdStr = re.Registration?.HorseId.ToString() ?? "";
                var raceStatus = re.Race?.Status ?? "Scheduled";
                
                // Determine finish position
                int finishPosition = re.FinishPosition ?? 0;
                if (finishPosition == 0 && raceStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase))
                {
                    finishPosition = 2; // Default for finished
                    if (winners.TryGetValue(re.RaceId, out var winner))
                    {
                        if (winner.Equals(horseName, StringComparison.OrdinalIgnoreCase) || winner == horseIdStr)
                        {
                            finishPosition = 1;
                        }
                    }
                }

                decimal prizeAmount = 0;
                if (raceStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase))
                {
                    if (re.Race?.Round?.RoundNumber == 2)
                    {
                        var prize = prizes.FirstOrDefault(p => p.TournamentId == re.Race.Round.TournamentId && p.RankPosition == finishPosition);
                        if (prize != null)
                        {
                            prizeAmount = prize.Amount * (prize.OwnerPercentage / 100m);
                        }
                    }
                    else if (finishPosition == 1)
                    {
                        // Fallback legacy support for pre-round winners showing a default win indicator
                        prizeAmount = 1000000;
                    }
                }

                return new {
                    RaceId = re.RaceId,
                    RaceName = re.Race?.Name ?? "",
                    TournamentName = re.Race?.Round?.Tournament?.Name ?? "",
                    HorseName = horseName,
                    FinishPosition = finishPosition,
                    FinishTime = raceStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                        ? (re.Race?.RaceDate.AddMinutes(5).ToString("HH:mm:ss") ?? "")
                        : "—",
                    Point = raceStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                        ? (finishPosition == 1 ? 10 : 5)
                        : 0,
                    PrizeAmount = prizeAmount,
                    Status = raceStatus
                };
            }).ToList();

            return Ok(new { message = "Results retrieved successfully", result = ownerResults });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving results", detail = ex.Message });
        }
    }

    [HttpGet("owner/dashboard")]
    public async Task<IActionResult> GetOwnerDashboard([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Horse count
            var horseCount = await context.Horses
                .Where(h => h.OwnerId == userId)
                .CountAsync();

            // Get horse IDs for this owner
            var horseIds = await context.Horses
                .Where(h => h.OwnerId == userId)
                .Select(h => h.HorseId)
                .ToListAsync();

            // Registration count
            var registrationCount = await context.Registrations
                .Where(r => horseIds.Contains(r.HorseId))
                .CountAsync();

            // Active race count (races where owner's horses are entered and status is not Finished)
            var activeRaceCount = await context.RaceEntries
                .Include(re => re.Race)
                .Include(re => re.Registration)
                .Where(re => horseIds.Contains(re.Registration.HorseId)
                    && re.Race != null
                    && (re.Race.Status == "Live" || re.Race.Status == "Running" || re.Race.Status == "InProgress"))
                .Select(re => re.RaceId)
                .Distinct()
                .CountAsync();

            // Upcoming race count
            var upcomingRaceCount = await context.RaceEntries
                .Include(re => re.Race)
                .Include(re => re.Registration)
                .Where(re => horseIds.Contains(re.Registration.HorseId)
                    && re.Race != null
                    && re.Race.Status == "Scheduled")
                .Select(re => re.RaceId)
                .Distinct()
                .CountAsync();

            // Total prize amount from payouts
            var totalPrizeAmount = await context.TournamentPrizePayouts
                .Where(tpp => tpp.UserId == userId)
                .SumAsync(tpp => (decimal?)tpp.Amount) ?? 0;

            var dashboard = new
            {
                HorseCount = horseCount,
                RegistrationCount = registrationCount,
                ActiveRaceCount = activeRaceCount,
                UpcomingRaceCount = upcomingRaceCount,
                TotalPrizeAmount = totalPrizeAmount
            };

            return Ok(new { message = "Owner dashboard retrieved successfully", result = dashboard });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving owner dashboard", detail = ex.Message });
        }
    }

    [HttpGet("owner/wallet/balance")]
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

    [HttpGet("owner/wallet/history")]
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

    [HttpPost("owner/wallet/deposit")]
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

    [HttpPost("owner/wallet/withdraw")]
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
}
