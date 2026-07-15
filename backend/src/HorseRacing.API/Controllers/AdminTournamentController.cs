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
                return BadRequest("Số ngày gia hạn phải lớn hơn 0.");
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound("Không tìm thấy giải đấu.");
            }

            if (tournament.CancelCount != 0)
            {
                return BadRequest("Giải đấu đã qua lượt gia hạn đầu tiên hoặc trạng thái không hợp lệ.");
            }

            if (!tournament.RegistrationEndDate.HasValue || !tournament.StartDate.HasValue)
            {
                return BadRequest("Thời gian đăng ký hoặc thời gian bắt đầu giải đấu chưa được cấu hình.");
            }

            // Tính toán ngày kết thúc đăng ký mới
            DateTime newRegistrationEndDate = tournament.RegistrationEndDate.Value.AddDays(request.AdditionalDays);

            // Validation Ràng buộc: Phải cách ngày bắt đầu giải đấu ít nhất 1 ngày trước khi đua
            if (newRegistrationEndDate > tournament.StartDate.Value.AddDays(-1))
            {
                return BadRequest("Ngày gia hạn vượt quá giới hạn cho phép. Thời gian đóng đăng ký mới phải cách ngày bắt đầu giải đấu ít nhất 1 ngày.");
            }

            // Cập nhật thông tin giải đấu
            tournament.RegistrationEndDate = newRegistrationEndDate;
            tournament.CancelCount = 1;
            tournament.Status = "Registration Open"; // Mở lại trạng thái đăng ký

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Gia hạn thời gian đăng ký giải đấu thành công.", NewRegistrationEndDate = tournament.RegistrationEndDate });
        }
    }
}
