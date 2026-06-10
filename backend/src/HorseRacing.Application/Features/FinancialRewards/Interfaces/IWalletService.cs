using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.DTOs;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IWalletService
{
    Task<WalletBalanceResponse> GetBalanceAsync(int userId);
    Task<WalletBalanceResponse> DepositAsync(int userId, DepositRequest request);
    Task<WalletBalanceResponse> WithdrawAsync(int userId, WithdrawRequest request);
    Task<IEnumerable<TransactionHistoryResponse>> GetTransactionHistoryAsync(int userId);
}
