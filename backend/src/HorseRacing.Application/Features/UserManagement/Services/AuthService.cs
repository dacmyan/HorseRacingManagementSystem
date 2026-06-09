using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HorseRacing.Application.Features.UserManagement.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordHasher<AppUser> _passwordHasher;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = new PasswordHasher<AppUser>();
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Message = "Login successful",
            Result = new AuthResult
            {
                AccessToken = token,
                RefreshToken = null,
                User = new UserDto
                {
                    Id = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role?.Name ?? "Spectator"
                }
            }
        };
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Full name is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters.");
        }
        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Confirm password does not match.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email already exists.");
        }

        var newUser = new AppUser
        {
            Username = request.Email.Split('@')[0],
            Email = request.Email,
            FullName = request.FullName,
            RoleId = 5 // Spectator role
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(newUser);

        return new AuthResponse
        {
            Message = "Register successful",
            Result = new AuthResult
            {
                AccessToken = token,
                RefreshToken = null,
                User = new UserDto
                {
                    Id = newUser.UserId,
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    Role = "Spectator"
                }
            }
        };
    }
}