using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/referee")]
[Authorize(Roles = "Referee")]
public class RefereeController : ControllerBase
{
    private readonly IRefereeService _refereeService;
    private readonly IRaceResultService _resultService;

    public RefereeController(IRefereeService refereeService, IRaceResultService resultService)
    {
        _refereeService = refereeService;
        _resultService = resultService;
    }

    [HttpPost("violations")]
    public async Task<IActionResult> LogViolation([FromBody] LogViolationRequest request, [FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var referee = await context.RefereeProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
            
            if (referee == null)
            {
                return NotFound(new { message = "Referee profile not found for current user." });
            }
            
            request.RefereeId = referee.RefereeId;
            
            var response = await _refereeService.LogViolationAsync(request);
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred logging the violation", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/violations")]
    public async Task<IActionResult> GetRaceViolations([FromRoute] long raceId)
    {
        try
        {
            var response = await _refereeService.GetViolationsByRaceIdAsync(raceId);
            if (response == null)
            {
                return NotFound(new { message = $"Race with ID {raceId} was not found." });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred retrieving race violations", detail = ex.Message });
        }
    }

    [HttpGet("violations")]
    public async Task<IActionResult> GetViolations([FromServices] AppDbContext context)
    {
        try
        {
            // For simplicity and since we don't have a direct Referee -> Violation link easily accessible, 
            // returning all violations similar to Admin, or just violations for races this referee officiated.
            var userId = GetCurrentUserId();
            var referee = await context.RefereeProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
            
            if (referee == null)
            {
                return NotFound(new { message = "Referee profile not found" });
            }

            var assignedRaceIds = await context.RaceRefereeAssignments
                .Where(a => a.RefereeId == referee.RefereeId)
                .Select(a => a.RaceId)
                .ToListAsync();

            var violations = await context.Violations
                .Include(v => v.Race)
                .Where(v => assignedRaceIds.Contains(v.RaceId))
                .Select(v => new {
                    ViolationId = v.Id,
                    RaceId = v.RaceId,
                    RaceName = v.Race != null ? v.Race.Name : "",
                    Type = v.Description.Contains(":") ? v.Description.Split(':', StringSplitOptions.None)[0] : "Vi phạm",
                    Note = v.Description,
                    Penalty = v.Penalty,
                    CreatedAt = DateTime.UtcNow // Using UTC now since Violation entity lacks CreatedAt
                })
                .ToListAsync();
                
            return Ok(new { message = "Violations retrieved successfully", result = violations });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving violations", detail = ex.Message });
        }
    }

    [HttpPost("reports")]
    public async Task<IActionResult> SubmitReport([FromBody] CreateRefereeReportRequest request, [FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var referee = await context.RefereeProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
            if (referee == null)
            {
                return NotFound(new { message = "Referee profile not found for current user." });
            }
            request.RefereeId = referee.RefereeId;

            var response = await _refereeService.SubmitReportAsync(request);
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred submitting the report", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/reports")]
    public async Task<IActionResult> GetRaceReports([FromRoute] long raceId)
    {
        try
        {
            var response = await _refereeService.GetReportsByRaceIdAsync(raceId);
            if (response == null)
            {
                return NotFound(new { message = $"Race with ID {raceId} was not found." });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred retrieving race reports", detail = ex.Message });
        }
    }

    [HttpPost("races/{raceId}/results")]
    public async Task<IActionResult> SubmitResultRoute([FromRoute] long raceId, [FromBody] SubmitRaceResultRequest request)
    {
        try
        {
            request.RaceId = raceId;
            var response = await _resultService.SubmitResultAsync(request);
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred submitting the race result", detail = ex.Message });
        }
    }

    [HttpPost("results")]
    public async Task<IActionResult> SubmitResult([FromBody] SubmitRaceResultRequest request)
    {
        try
        {
            var response = await _resultService.SubmitResultAsync(request);
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred submitting the race result", detail = ex.Message });
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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred retrieving race results", detail = ex.Message });
        }
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

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromServices] AppDbContext context)
    {
        try
        {
            var userId = GetCurrentUserId();
            var referee = await context.RefereeProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (referee == null)
            {
                return NotFound(new { message = "Referee profile not found" });
            }

            var assignments = await context.RaceRefereeAssignments
                .Include(a => a.Race)
                .Where(a => a.RefereeId == referee.RefereeId)
                .ToListAsync();

            var assignmentIds = assignments.Select(a => a.AssignmentId).ToList();

            var reports = await context.RefereeReports
                .Where(r => assignmentIds.Contains(r.AssignmentId))
                .ToListAsync();

            var completedReportCount = reports.Count;
            var pendingReportCount = assignments.Count - completedReportCount;
            
            var assignedRaceIds = assignments.Select(a => a.RaceId).ToList();
            var violationsCreatedCount = await context.Violations
                .Where(v => assignedRaceIds.Contains(v.RaceId))
                .CountAsync();

            var assignedRaces = assignments.Select(a => new {
                RaceId = a.RaceId,
                RaceName = a.Race?.Name ?? "",
                RaceDate = a.Race?.RaceDate,
                Status = a.Race?.Status ?? "Scheduled"
            }).ToList();

            var result = new {
                AssignedRaceCount = assignments.Count,
                PendingReportCount = pendingReportCount,
                CompletedReportCount = completedReportCount,
                ViolationsCreatedCount = violationsCreatedCount,
                AssignedRaces = assignedRaces
            };

            return Ok(new { message = "Referee dashboard retrieved successfully", result = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving dashboard", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/horse-checks")]
    public async Task<IActionResult> GetHorseChecks(long raceId, [FromServices] AppDbContext context)
    {
        try
        {
            var entries = await context.RaceEntries
                .Include(re => re.Registration)
                    .ThenInclude(reg => reg.Horse)
                        .ThenInclude(h => h.Owner)
                .Include(re => re.JockeyProfile)
                    .ThenInclude(jp => jp.User)
                .Where(re => re.RaceId == raceId)
                .ToListAsync();

            var horseChecks = entries.Select(re => new {
                RaceEntryId = re.RaceEntryId,
                HorseId = re.Registration?.HorseId ?? 0,
                HorseName = re.Registration?.Horse?.Name ?? "",
                OwnerName = re.Registration?.Horse?.Owner?.FullName ?? "",
                JockeyName = re.JockeyProfile?.User?.FullName ?? "",
                LaneNo = re.LaneNo,
                MedicalStatus = re.Registration?.Horse?.HealthStatus ?? "Good",
                Status = re.Status
            }).ToList();

            return Ok(new { message = "Horse checks retrieved successfully", result = horseChecks });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving horse checks", detail = ex.Message });
        }
    }

    [HttpPut("violations/{id}")]
    public async Task<IActionResult> UpdateViolation(long id, [FromBody] UpdateViolationRequest request, [FromServices] AppDbContext context)
    {
        try
        {
            var violation = await context.Violations.FindAsync(id);
            if (violation == null)
            {
                return NotFound(new { message = $"Violation with ID {id} was not found." });
            }

            if (!string.IsNullOrEmpty(request.Penalty))
            {
                violation.Penalty = request.Penalty;
            }
            if (!string.IsNullOrEmpty(request.Description))
            {
                violation.Description = request.Description;
            }

            await context.SaveChangesAsync();

            return Ok(new { message = "Violation updated successfully", result = violation });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating the violation", detail = ex.Message });
        }
    }
}
