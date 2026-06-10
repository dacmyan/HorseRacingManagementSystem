using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task AddAsync(Wallet wallet)
    {
        await _context.Wallets.AddAsync(wallet);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
