using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.UserManagement.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByIdAsync(int id);
    Task AddAsync(AppUser user);
    Task SaveChangesAsync();
}