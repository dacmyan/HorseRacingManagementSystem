using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly INotificationRepository _notificationRepository;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        INotificationRepository notificationRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _notificationRepository = notificationRepository;
    }

    private async Task<Wallet> GetOrCreateWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            var userExists = await _walletRepository.UserExistsAsync(userId);
            if (!userExists)
            {
                throw new ArgumentException($"User with ID {userId} does not exist.");
            }

            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();
        }
        return wallet;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(int userId)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        return new WalletBalanceResponse
        {
            WalletId = wallet.WalletId,
            UserId = wallet.UserId,
            Balance = wallet.Balance
        };
    }

    public async Task<WalletBalanceResponse> DepositAsync(int userId, DepositRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.");
        }

        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Balance += request.Amount;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = request.Amount,
            Type = "Deposit",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // Create system notification
        var notification = new Notification
        {
            UserId = userId,
            Message = $"You have successfully deposited {request.Amount:N2} to your wallet. New balance: {wallet.Balance:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return new WalletBalanceResponse
        {
            WalletId = wallet.WalletId,
            UserId = wallet.UserId,
            Balance = wallet.Balance
        };
    }

    public async Task<WalletBalanceResponse> WithdrawAsync(int userId, WithdrawRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero.");
        }

        var wallet = await GetOrCreateWalletAsync(userId);
        if (wallet.Balance < request.Amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        wallet.Balance -= request.Amount;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = -request.Amount,
            Type = "Withdraw",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        // Create system notification
        var notification = new Notification
        {
            UserId = userId,
            Message = $"You have successfully withdrawn {request.Amount:N2} from your wallet. New balance: {wallet.Balance:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return new WalletBalanceResponse
        {
            WalletId = wallet.WalletId,
            UserId = wallet.UserId,
            Balance = wallet.Balance
        };
    }

    public async Task<IEnumerable<TransactionHistoryResponse>> GetTransactionHistoryAsync(int userId)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        var transactions = await _transactionRepository.GetByWalletIdAsync(wallet.WalletId);

        return transactions.Select(t => new TransactionHistoryResponse
        {
            Id = t.TransactionId,
            WalletId = t.WalletId,
            Amount = t.Amount,
            Type = t.Type,
            CreatedAt = t.CreatedAt
        });
    }
}
