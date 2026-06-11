using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.HorseManagement.DTOs;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public OwnerController(
        IHorseService horseService,
        IHorseDocumentService horseDocumentService,
        IJockeyContractService jockeyContractService,
        IRegistrationService registrationService)
    {
        _horseService = horseService;
        _horseDocumentService = horseDocumentService;
        _jockeyContractService = jockeyContractService;
        _registrationService = registrationService;
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
}
