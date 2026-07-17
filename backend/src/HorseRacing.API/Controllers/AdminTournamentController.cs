using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HorseRacing.Infrastructure.Persistence;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using System;
using System.Threading.Tasks;

namespace HorseRacing.API.Controllers
{
    [ApiController]
    [Route("api/admin/tournaments")]
    [Authorize(Roles = "Admin")]
    public class AdminTournamentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminTournamentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPut("{id}/extend")]
        public async Task<IActionResult> ExtendRegistration(long id, [FromBody] ExtendRegistrationRequest request)
        {
            if (request == null || request.AdditionalDays <= 0)
            {
                return BadRequest("Additional days must be greater than 0.");
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            if (tournament.CancelCount != 0)
            {
                return BadRequest("The tournament has already been extended once or has an invalid status.");
            }

            if (!tournament.RegistrationEndDate.HasValue || !tournament.StartDate.HasValue)
            {
                return BadRequest("Registration or start date has not been configured.");
            }

            // Tính toán ngày kết thúc đăng ký mới từ thời gian hiện tại
            DateTime baseDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
            DateTime newRegistrationEndDate = baseDate.AddDays(request.AdditionalDays);

            // Validation Ràng buộc: Phải cách ngày bắt đầu giải đấu ít nhất 2 ngày trước khi đua
            if (newRegistrationEndDate > tournament.StartDate.Value.AddDays(-2))
            {
                return BadRequest("The extended registration date exceeds the limit. The new registration end date must be at least 2 days before the tournament starts.");
            }

            // Cập nhật thông tin giải đấu
            tournament.RegistrationEndDate = newRegistrationEndDate;
            tournament.CancelCount = 1;
            tournament.Status = "Registration Open"; // Mở lại trạng thái đăng ký

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registration period extended successfully.", NewRegistrationEndDate = tournament.RegistrationEndDate });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelTournament(long id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            tournament.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tournament cancelled successfully." });
        }
    }
}
