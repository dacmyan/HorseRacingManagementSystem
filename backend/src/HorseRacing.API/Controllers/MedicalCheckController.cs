using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.DTOs;
using HorseRacing.Application.Features.MedicalCheck.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacing.API.Controllers;

/// <summary>
/// CRUD for horse medical check records.
/// - Admin / Veterinarian can create, update, and list all records.
/// - Veterinarian can perform re-examinations which may trigger horse withdrawal.
/// - HorseOwner can list records by their registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalCheckController : ControllerBase
{
    private readonly IMedicalCheckService _service;

    public MedicalCheckController(IMedicalCheckService service)
    {
        _service = service;
    }

    private int GetCurrentUserId()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier))
            nameIdentifier = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        return int.Parse(nameIdentifier ?? "0");
    }

    // ─── GET /api/MedicalCheck ────────────────────────────────────────────────
    /// <summary>Get all medical check records (Admin / Veterinarian only).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(new { message = "Medical check records retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving records", detail = ex.Message });
        }
    }

    // ─── GET /api/MedicalCheck/{id} ───────────────────────────────────────────
    /// <summary>Get a single medical check record by ID.</summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null)
                return NotFound(new { message = $"Medical check record #{id} not found." });

            return Ok(new { message = "Medical check record retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    // ─── GET /api/MedicalCheck/by-registration/{registrationId} ──────────────
    /// <summary>Get all medical check records for a specific horse registration.</summary>
    [HttpGet("by-registration/{registrationId:long}")]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetByRegistration(long registrationId)
    {
        try
        {
            var result = await _service.GetByRegistrationIdAsync(registrationId);
            return Ok(new { message = "Medical check records retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    // ─── GET /api/MedicalCheck/pending-registrations ─────────────────────────
    /// <summary>Get all approved registrations that need an initial medical check.</summary>
    [HttpGet("pending-registrations")]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetPendingRegistrations()
    {
        try
        {
            var result = await _service.GetPendingRegistrationsAsync();
            return Ok(new { message = "Pending registrations retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving pending registrations", detail = ex.Message });
        }
    }

    // ─── GET /api/MedicalCheck/assigned-entries ───────────────────────────────
    /// <summary>
    /// Get all horses currently assigned to a race that are eligible for re-examination.
    /// Returns entries with their last medical check info for the vet to review.
    /// </summary>
    [HttpGet("assigned-entries")]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetAssignedEntries()
    {
        try
        {
            var result = await _service.GetAssignedRaceEntriesAsync();
            return Ok(new { message = "Assigned race entries retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving assigned entries", detail = ex.Message });
        }
    }

    // ─── POST /api/MedicalCheck ───────────────────────────────────────────────
    /// <summary>Create a new initial medical check record (Veterinarian only).</summary>
    [HttpPost]
    [Authorize(Roles = "Veterinarian")]
    public async Task<IActionResult> Create([FromBody] CreateMedicalCheckRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.CreateAsync(userId, request);
            return Ok(new { message = "Medical check record created successfully", result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred creating the record", detail = ex.Message });
        }
    }

    // ─── POST /api/MedicalCheck/recheck ──────────────────────────────────────
    /// <summary>
    /// Perform a re-examination on a horse already assigned to a race.
    /// If the result is Fail:
    ///   - Creates a new MedicalCheckRecord (CheckType = "ReCheck")
    ///   - Sets Registration.Status to "Disqualified"
    ///   - Sets RaceEntry.Status to "Withdrawn" (pre-race) or "DNF" (mid-race)
    ///   - Sends notifications to owner, jockey, referees, and bettors
    /// </summary>
    [HttpPost("recheck")]
    [Authorize(Roles = "Veterinarian")]
    public async Task<IActionResult> PerformRecheck([FromBody] RecheckMedicalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.PerformRecheckAsync(userId, request);

            var statusCode = result.HorseWithdrawn ? 200 : 200;
            return Ok(new { message = result.Message, result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred performing the re-examination", detail = ex.Message });
        }
    }

    // ─── PUT /api/MedicalCheck/{id} ───────────────────────────────────────────
    /// <summary>Update an existing medical check record (Veterinarian only).</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Veterinarian")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateMedicalCheckRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(new { message = "Medical check record updated successfully", result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating the record", detail = ex.Message });
        }
    }

    // ─── GET /api/MedicalCheck/unhealthy-horses ──────────────────────────────────
    /// <summary>Get all unhealthy (Sick/Injured) horses (Admin / Veterinarian only).</summary>
    [HttpGet("unhealthy-horses")]
    [Authorize(Roles = "Admin,Veterinarian")]
    public async Task<IActionResult> GetUnhealthyHorses()
    {
        try
        {
            var result = await _service.GetUnhealthyHorsesAsync();
            return Ok(new { message = "Unhealthy horses retrieved successfully", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred retrieving unhealthy horses", detail = ex.Message });
        }
    }

    // ─── PUT /api/MedicalCheck/horses/{id}/recover ──────────────────────────────
    /// <summary>Confirm recovery of an unhealthy horse, setting health status to Healthy (Veterinarian only).</summary>
    [HttpPut("horses/{id:long}/recover")]
    [Authorize(Roles = "Veterinarian")]
    public async Task<IActionResult> RecoverHorse(long id)
    {
        try
        {
            var updated = await _service.RecoverHorseAsync(id);
            if (!updated)
            {
                return BadRequest(new { message = "Horse is already healthy." });
            }
            return Ok(new { message = "Horse health status updated to Healthy successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred updating horse health status", detail = ex.Message });
        }
    }

    // ─── POST /api/MedicalCheck/horses/{id}/request-recovery ───────────────────
    /// <summary>Request health recovery check for a sick/injured horse (HorseOwner only).</summary>
    [HttpPost("horses/{id:long}/request-recovery")]
    [Authorize(Roles = "HorseOwner")]
    public async Task<IActionResult> RequestRecoveryCheck(long id)
    {
        try
        {
            var ownerUserId = GetCurrentUserId();
            var result = await _service.RequestRecoveryCheckAsync(ownerUserId, id);
            return Ok(new { message = "Recovery check request submitted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred submitting recovery check request", detail = ex.Message });
        }
    }
}
