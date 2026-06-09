using HorseRacing.Application.Features.UserManagement.DTOs;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HorseRacing.Application.Features.UserManagement.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<AppUser> _passwordHasher;

    public AdminService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = new PasswordHasher<AppUser>();
    }

    public async Task<CreateAccountResponseDto> CreateAccountAsync(CreateAccountRequestDto request)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");

        if (string.IsNullOrWhiteSpace(request.Role))
            throw new ArgumentException("Role is required.");

        if (request.Role.Equals("Referee", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ArgumentException("License number is required for Referee.");

        // 2. Uniqueness check
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ArgumentException("Email already exists.");

        // 3. Role check
        var dbRole = await _userRepository.GetRoleByNameAsync(request.Role);
        if (dbRole == null)
            throw new ArgumentException($"Role '{request.Role}' does not exist.");

        // 4. Create AppUser
        var newUser = new AppUser
        {
            FullName = request.FullName,
            Email = request.Email,
            Username = request.Email.Split('@')[0],
            RoleId = dbRole.RoleId,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync(); // Commit so newUser gets a UserId

        // 5. Create Profile / Wallet
        if (dbRole.Name.Equals("Jockey", StringComparison.OrdinalIgnoreCase))
        {
            var jockeyProfile = new JockeyProfile
            {
                UserId = newUser.UserId,
                ExperienceYears = request.ExperienceYears ?? 0,
                RankingPoint = 0,
                Status = "Active"
            };
            await _userRepository.AddJockeyProfileAsync(jockeyProfile);
        }
        else if (dbRole.Name.Equals("Referee", StringComparison.OrdinalIgnoreCase))
        {
            var refereeProfile = new RefereeProfile
            {
                UserId = newUser.UserId,
                LicenseNumber = request.LicenseNumber!,
                ExperienceYears = request.ExperienceYears ?? 0,
                Status = "Active"
            };
            await _userRepository.AddRefereeProfileAsync(refereeProfile);
        }
        else if (dbRole.Name.Equals("Spectator", StringComparison.OrdinalIgnoreCase))
        {
            var wallet = new Wallet
            {
                UserId = newUser.UserId,
                Balance = 0
            };
            await _userRepository.AddWalletAsync(wallet);
        }

        await _userRepository.SaveChangesAsync();

        // 6. Return response DTO
        return new CreateAccountResponseDto
        {
            UserId = newUser.UserId,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = dbRole.Name,
            Status = newUser.Status
        };
    }

    public async Task<IEnumerable<RoleResponseDto>> GetRolesAsync()
    {
        var roles = await _userRepository.GetRolesAsync();
        return roles.Select(r => new RoleResponseDto
        {
            RoleId = r.RoleId,
            Name = r.Name
        });
    }
}
