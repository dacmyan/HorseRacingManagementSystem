using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.UserManagement.DTOs;

public class CreateAccountRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    [Required, StringLength(30)]
    public string Role { get; set; } = string.Empty;
    [StringLength(100)]
    public string? LicenseNumber { get; set; }
    [Range(0, 80)]
    public int? ExperienceYears { get; set; }
}
