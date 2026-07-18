namespace HorseRacing.Application.Features.UserManagement.DTOs;

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;
    public AuthResult Result { get; set; } = null!;
}

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}