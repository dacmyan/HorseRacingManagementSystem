using HorseRacing.Application.Features.UserManagement.DTOs;

namespace HorseRacing.Application.Features.UserManagement.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponse?> GoogleLoginAsync(GoogleLoginRequest request);
    Task<bool> VerifyEmailAsync(string email, string token);
}
