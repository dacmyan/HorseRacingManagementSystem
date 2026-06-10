using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IWalletTransactionRepository
{
    Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(int walletId);
    Task AddAsync(WalletTransaction transaction);
    Task SaveChangesAsync();
}
