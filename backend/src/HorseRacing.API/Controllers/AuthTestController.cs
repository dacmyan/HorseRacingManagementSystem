using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminTest()
    {
        return Ok(new
        {
            message = "Admin authorization successful",
            role = "Admin"
        });
    }

    [HttpGet("horse-owner")]
    [Authorize(Roles = "HorseOwner")]
    public IActionResult GetHorseOwnerTest()
    {
        return Ok(new
        {
            message = "HorseOwner authorization successful",
            role = "HorseOwner"
        });
    }

    [HttpGet("jockey")]
    [Authorize(Roles = "Jockey")]
    public IActionResult GetJockeyTest()
    {
        return Ok(new
        {
            message = "Jockey authorization successful",
            role = "Jockey"
        });
    }

    [HttpGet("referee")]
    [Authorize(Roles = "Referee")]
    public IActionResult GetRefereeTest()
    {
        return Ok(new
        {
            message = "Referee authorization successful",
            role = "Referee"
        });
    }

    [HttpGet("spectator")]
    [Authorize(Roles = "Spectator")]
    public IActionResult GetSpectatorTest()
    {
        return Ok(new
        {
            message = "Spectator authorization successful",
            role = "Spectator"
        });
    }

    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult GetAuthenticatedTest()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "None";
        return Ok(new
        {
            message = "Authenticated authorization successful",
            role = roleClaim
        });
    }
}
