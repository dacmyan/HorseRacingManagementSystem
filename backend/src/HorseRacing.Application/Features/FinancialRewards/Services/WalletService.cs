using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        INotificationService notificationService,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _emailService = emailService;
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

        // Create wallet notification
        try
        {
            await _notificationService.SendNotificationToUserAsync(
                userId,
                "Deposit Successful",
                $"You successfully deposited {request.Amount:N2}$ into your wallet. New balance: {wallet.Balance:N2}$.",
                "Wallet",
                actionUrl: "/spectator/wallet"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WalletService.DepositAsync] Error sending notification: {ex}");
        }

        // Send Email Receipt
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var subject = "Biên lai Nạp Tiền: Nạp tiền thành công";
                var body = $@"
                    <h2>Nạp tiền thành công!</h2>
                    <p>Xin chào {user.FullName},</p>
                    <p>Bạn đã nạp thành công <strong>{request.Amount:N2}$</strong> vào ví của mình.</p>
                    <p>Số dư mới của bạn là: <strong>{wallet.Balance:N2}$</strong>.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                ";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WalletService.DepositAsync] Error sending email receipt: {ex}");
        }

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

        // Create wallet notification
        try
        {
            await _notificationService.SendNotificationToUserAsync(
                userId,
                "Withdrawal Successful",
                $"You successfully withdrew {request.Amount:N2}$ from your wallet. New balance: {wallet.Balance:N2}$.",
                "Wallet",
                actionUrl: "/spectator/wallet"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WalletService.WithdrawAsync] Error sending notification: {ex}");
        }

        // Send Email Receipt
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var subject = "Biên lai Rút Tiền: Rút tiền thành công";
                var body = $@"
                    <h2>Rút tiền thành công!</h2>
                    <p>Xin chào {user.FullName},</p>
                    <p>Bạn đã rút thành công <strong>{request.Amount:N2}$</strong> từ ví của mình.</p>
                    <p>Số dư mới của bạn là: <strong>{wallet.Balance:N2}$</strong>.</p>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                ";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WalletService.WithdrawAsync] Error sending email receipt: {ex}");
        }

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
            Amount = t.PaymentMethod == "VNPay" ? t.Amount / 250m : t.Amount,
            Type = t.Type,
            Description = t.Description,
            CreatedAt = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc)
        });
    }
}
