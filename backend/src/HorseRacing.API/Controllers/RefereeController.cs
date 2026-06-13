using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/Referee")]
[Authorize(Roles = "Referee")]
public class RefereeController : ControllerBase
{
    private readonly IRaceViolationService _raceViolationService;

    public RefereeController(IRaceViolationService raceViolationService)
    {
        _raceViolationService = raceViolationService;
    }

    [HttpPost("races/{raceId}/violations")]
    public async Task<IActionResult> CreateViolation(long raceId, [FromBody] CreateRaceViolationRequest request)
    {
        try
        {
            var result = await _raceViolationService.CreateViolationAsync(raceId, request);
            return Ok(new { message = "Violation logged successfully", result = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
            return StatusCode(500, new { message = "An error occurred logging the violation", detail = ex.Message });
        }
    }

    [HttpGet("races/{raceId}/violations")]
    public async Task<IActionResult> GetViolations(long raceId)
    {
        try
        {
            var result = await _raceViolationService.GetViolationsByRaceIdAsync(raceId);
            return Ok(new { message = "Violations retrieved successfully", result = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
            return StatusCode(500, new { message = "An error occurred retrieving violations", detail = ex.Message });
        }
    }
}
