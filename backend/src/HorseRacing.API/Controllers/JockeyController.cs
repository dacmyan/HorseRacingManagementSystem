using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/jockeys")]
[Authorize(Roles = "Jockey")]
public class JockeyController : ControllerBase
{
    private readonly IJockeyContractService _jockeyContractService;

    public JockeyController(IJockeyContractService jockeyContractService)
    {
        _jockeyContractService = jockeyContractService;
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

    [HttpGet("contracts")]
    public async Task<IActionResult> GetMyContracts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _jockeyContractService.GetContractsForJockeyAsync(userId);
            return Ok(new { message = "Your contract proposals retrieved successfully", result = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving your contracts", detail = ex.Message });
        }
    }

    [HttpPut("contracts/{id}/respond")]
    public async Task<IActionResult> RespondToContract(int id, [FromBody] RespondToContractRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _jockeyContractService.RespondToContractAsync(userId, id, request);
            return Ok(new { message = $"Contract successfully updated to '{request.Status}'", result = response });
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
            return StatusCode(500, new { message = "An error occurred responding to the contract", detail = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetJockeyStats([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jockey = await context.JockeyProfiles
                .FirstOrDefaultAsync(jp => jp.UserId == userId);

            if (jockey == null)
            {
                return NotFound(new { message = "Jockey profile not found" });
            }

            // Count total races from RaceEntry
            var entries = await context.RaceEntries
                .Include(re => re.Race)
                .Include(re => re.Registration)
                    .ThenInclude(reg => reg.Horse)
                .Where(re => re.JockeyId == jockey.JockeyId)
                .ToListAsync();

            var raceIds = entries.Select(re => re.RaceId).ToList();

            var results = await context.RaceResults
                .Where(rr => raceIds.Contains(rr.RaceId))
                .ToListAsync();

            int wins = 0;
            int top3 = 0;
            foreach (var entry in entries)
            {
                bool isWin = entry.FinishPosition == 1;
                if (!isWin)
                {
                    var result = results.FirstOrDefault(r => r.RaceId == entry.RaceId);
                    if (result != null && entry.Registration?.Horse != null)
                    {
                        if (result.Winner.Equals(entry.Registration.Horse.Name, StringComparison.OrdinalIgnoreCase) ||
                            result.Winner == entry.Registration.HorseId.ToString())
                        {
                            isWin = true;
                        }
                    }
                }

                if (isWin)
                {
                    wins++;
                    top3++;
                }
                else if (entry.FinishPosition == 2 || entry.FinishPosition == 3)
                {
                    top3++;
                }
            }

            var resultStats = new {
                TotalRaces = entries.Count,
                Wins = wins,
                Top3 = top3,
                TotalPoints = wins * 10,
                RankingPoint = jockey.RankingPoint
            };

            return Ok(new { message = "Jockey stats retrieved successfully", result = resultStats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving jockey stats", detail = ex.Message });
        }
    }

    [HttpGet("violations")]
    public async Task<IActionResult> GetJockeyViolations([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jockey = await context.JockeyProfiles
                .FirstOrDefaultAsync(jp => jp.UserId == userId);

            if (jockey == null)
            {
                return NotFound(new { message = "Jockey profile not found" });
            }

            var raceIds = await context.RaceEntries
                .Where(re => re.JockeyId == jockey.JockeyId)
                .Select(re => re.RaceId)
                .ToListAsync();

            var violations = await context.Violations
                .Include(v => v.Race)
                .Where(v => raceIds.Contains(v.RaceId))
                .Select(v => new {
                    ViolationId = v.Id,
                    RaceName = v.Race != null ? v.Race.Name : "",
                    Type = v.Description.Contains(":") ? v.Description.Split(':', StringSplitOptions.None)[0] : "Violation",
                    Note = v.Description,
                    Penalty = v.Penalty,
                    CreatedAt = DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(new { message = "Jockey violations retrieved successfully", result = violations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving violations", detail = ex.Message });
        }
    }

    [HttpGet("assigned-horses")]
    public async Task<IActionResult> GetAssignedHorses([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jockey = await context.JockeyProfiles
                .FirstOrDefaultAsync(jp => jp.UserId == userId);

            if (jockey == null)
            {
                return NotFound(new { message = "Jockey profile not found" });
            }

            // We can get assigned horses from accepted contracts and Race Entries.
            // Let's get it from Race Entries where Jockey is assigned.
            var assignments = await context.RaceEntries
                .Include(re => re.Race)
                .Include(re => re.Registration)
                    .ThenInclude(reg => reg.Horse)
                .Include(re => re.Registration)
                    .ThenInclude(reg => reg.Tournament)
                .Where(re => re.JockeyId == jockey.JockeyId)
                .Select(re => new {
                    RaceEntryId = re.RaceEntryId,
                    RaceId = re.RaceId,
                    RaceName = re.Race != null ? re.Race.Name : "",
                    RaceDate = re.Race != null ? re.Race.RaceDate : (DateTime?)null,
                    HorseId = re.Registration != null ? re.Registration.HorseId : 0,
                    HorseName = (re.Registration != null && re.Registration.Horse != null) ? re.Registration.Horse.Name : "",
                    TournamentName = (re.Registration != null && re.Registration.Tournament != null) ? re.Registration.Tournament.Name : "",
                    LaneNo = re.LaneNo,
                    Status = re.Race != null ? re.Race.Status : re.Status,
                    FinishPosition = re.FinishPosition,
                    FinishTime = re.FinishTime
                })
                .ToListAsync();

            return Ok(new { message = "Assigned horses retrieved successfully", result = assignments });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving assigned horses", detail = ex.Message });
        }
    }
}
