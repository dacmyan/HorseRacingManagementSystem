namespace HorseRacing.Application.Features.UserManagement.DTOs;

public class CreateAccountRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public int? ExperienceYears { get; set; }
}
