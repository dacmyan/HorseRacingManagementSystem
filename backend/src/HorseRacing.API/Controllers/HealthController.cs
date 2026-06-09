using Microsoft.AspNetCore.Mvc;
using HorseRacing.Infrastructure.Persistence;

namespace HorseRacing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("db")]
    public async Task<IActionResult> TestDbConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { status = "success", message = "Database connected successfully" });
            }
            return StatusCode(500, new { status = "error", message = "Cannot connect to database", detail = "Database creator check returned false" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = "Cannot connect to database", detail = ex.Message });
        }
    }
}
