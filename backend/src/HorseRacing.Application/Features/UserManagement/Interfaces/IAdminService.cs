using HorseRacing.Application.Features.UserManagement.DTOs;

namespace HorseRacing.Application.Features.UserManagement.Interfaces;

public interface IAdminService
{
    Task<CreateAccountResponseDto> CreateAccountAsync(CreateAccountRequestDto request);
    Task<IEnumerable<RoleResponseDto>> GetRolesAsync();
    Task<IEnumerable<AccountResponseDto>> GetAccountsAsync();
}
