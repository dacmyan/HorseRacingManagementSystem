using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/referee")]
[Authorize(Roles = "Referee")]
public class RefereeController : ControllerBase
{
    private readonly IRefereeService _refereeService;

    public RefereeController(IRefereeService refereeService)
    {
        _refereeService = refereeService;
    }

    [HttpPost("violations")]
    public async Task<IActionResult> LogViolation([FromBody] LogViolationRequest request)
    {
        try
        {
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

    [HttpPost("reports")]
    public async Task<IActionResult> SubmitReport([FromBody] CreateRefereeReportRequest request)
    {
        try
        {
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
}
