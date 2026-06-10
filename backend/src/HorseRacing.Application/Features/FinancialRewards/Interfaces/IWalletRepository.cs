using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(int userId);
    Task<bool> UserExistsAsync(int userId);
    Task AddAsync(Wallet wallet);
    Task SaveChangesAsync();
}
