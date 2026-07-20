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
    private readonly IGoogleTokenVerifier _googleTokenVerifier;
    private readonly IEmailService _emailService;
    private readonly PasswordHasher<AppUser> _passwordHasher;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IGoogleTokenVerifier googleTokenVerifier, IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _googleTokenVerifier = googleTokenVerifier;
        _emailService = emailService;
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

        if (!user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email của bạn để thực hiện xác thực.");
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
                    Role = user.Role?.Name ?? "Spectator",
                    Status = user.Status
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

        var verificationToken = Guid.NewGuid().ToString();
        var tokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        var newUser = new AppUser
        {
            Username = request.Email.Split('@')[0],
            Email = request.Email,
            FullName = request.FullName,
            RoleId = 5, // Spectator role
            IsEmailConfirmed = false,
            VerificationToken = verificationToken,
            TokenExpiresAt = tokenExpiresAt
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        try
        {
            var verificationLink = $"https://localhost:55445/api/auth/verify-email?email={Uri.EscapeDataString(newUser.Email)}&token={Uri.EscapeDataString(verificationToken)}";
            var htmlBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;"">
    <h2 style=""color: #333; text-align: center;"">Xác Thực Tài Khoản Đăng Ký</h2>
    <p>Xin chào <strong>{newUser.FullName}</strong>,</p>
    <p>Cảm ơn bạn đã đăng ký tài khoản tại hệ thống Horse Racing Management System.</p>
    <p>Vui lòng click vào liên kết bên dưới để xác thực và kích hoạt tài khoản của bạn (liên kết có hiệu lực trong vòng 15 phút):</p>
    <div style=""text-align: center; margin: 30px 0;"">
        <a href=""{verificationLink}"" style=""background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Xác Thực Email</a>
    </div>
    <p>Nếu nút trên không hoạt động, bạn cũng có thể sao chép liên kết dưới đây vào trình duyệt của mình:</p>
    <p style=""word-break: break-all;""><a href=""{verificationLink}"">{verificationLink}</a></p>
    <hr style=""border: 0; border-top: 1px solid #eee; margin: 20px 0;"" />
    <p style=""font-size: 12px; color: #777; text-align: center;"">Đây là email tự động từ hệ thống. Vui lòng không phản hồi lại email này.</p>
</div>";
            await _emailService.SendEmailAsync(newUser.Email, "Xác thực tài khoản Horse Racing Management System", htmlBody);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Đăng ký thành công nhưng không thể gửi email xác thực. Chi tiết lỗi SMTP: {ex.Message}", ex);
        }

        return new AuthResponse
        {
            Message = "Đăng ký thành công. Vui lòng kiểm tra email của bạn để thực hiện xác thực và kích hoạt tài khoản.",
            Result = new AuthResult
            {
                AccessToken = string.Empty,
                RefreshToken = null,
                User = new UserDto
                {
                    Id = newUser.UserId,
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    Role = "Spectator",
                    Status = newUser.Status
                }
            }
        };
    }

    public async Task<AuthResponse?> GoogleLoginAsync(GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new ArgumentException("IdToken is required.");
        }

        var googleUser = await _googleTokenVerifier.VerifyTokenAsync(request.IdToken);
        if (googleUser == null)
        {
            return null;
        }

        var email = googleUser.Email;
        var existingUser = await _userRepository.GetByEmailAsync(email);

        AppUser user;
        if (existingUser != null)
        {
            var roleName = existingUser.Role?.Name ?? string.Empty;
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                roleName.Equals("Referee", StringComparison.OrdinalIgnoreCase) || 
                roleName.Equals("RaceReferee", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Tài khoản thuộc nhóm quản trị hệ thống không được phép liên kết tự động bằng Google Login.");
            }

            user = existingUser;
        }
        else
        {
            user = new AppUser
            {
                Username = email.Split('@')[0],
                Email = email,
                FullName = string.IsNullOrWhiteSpace(googleUser.Name) ? email.Split('@')[0] : googleUser.Name,
                RoleId = 5, // Spectator role
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = string.Empty
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Fetch user with role information populated
            user = await _userRepository.GetByEmailAsync(email) ?? user;
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Message = "Google login successful",
            Result = new AuthResult
            {
                AccessToken = token,
                RefreshToken = null,
                User = new UserDto
                {
                    Id = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role?.Name ?? "Spectator",
                    Status = user.Status
                }
            }
        };
    }

    public async Task<bool> VerifyEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        if (user.VerificationToken != token)
        {
            return false;
        }

        if (user.TokenExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        user.IsEmailConfirmed = true;
        user.VerificationToken = null;
        user.TokenExpiresAt = null;

        await _userRepository.SaveChangesAsync();
        return true;
    }
}