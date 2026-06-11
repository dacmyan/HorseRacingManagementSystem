using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
