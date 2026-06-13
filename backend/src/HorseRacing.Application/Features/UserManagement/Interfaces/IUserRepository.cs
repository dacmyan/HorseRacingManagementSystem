using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.UserManagement.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByIdAsync(int id);
    Task AddAsync(AppUser user);
    Task SaveChangesAsync();

    Task<Role?> GetRoleByNameAsync(string name);
    Task<IEnumerable<Role>> GetRolesAsync();
    Task<IEnumerable<AppUser>> GetAllUsersAsync();
    Task AddJockeyProfileAsync(JockeyProfile profile);
    Task AddRefereeProfileAsync(RefereeProfile profile);
    Task AddWalletAsync(Wallet wallet);
}