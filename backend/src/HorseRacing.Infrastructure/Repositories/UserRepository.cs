using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetByIdAsync(int id)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
    {
        return await _context.Users.Include(u => u.Role).ToListAsync();
    }

    public async Task AddJockeyProfileAsync(JockeyProfile profile)
    {
        await _context.JockeyProfiles.AddAsync(profile);
    }

    public async Task AddRefereeProfileAsync(RefereeProfile profile)
    {
        await _context.RefereeProfiles.AddAsync(profile);
    }

    public async Task AddWalletAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
    }
}