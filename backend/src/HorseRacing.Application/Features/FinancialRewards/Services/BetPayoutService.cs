using System;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class BetPayoutService : IBetPayoutService
{
    private readonly IBetRepository _betRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IPayoutRepository _payoutRepository;
    private readonly INotificationRepository _notificationRepository;

    public BetPayoutService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPayoutRepository payoutRepository,
        INotificationRepository notificationRepository)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _payoutRepository = payoutRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task ProcessPayoutAsync(long raceId)
    {
        var race = await _betRepository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new ArgumentException($"Race with ID {raceId} not found.");
        }

        var result = await _betRepository.GetRaceResultAsync(raceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            throw new InvalidOperationException($"No published results or winner found for race ID {raceId}.");
        }

        // Try to resolve the winning horse by ID or by Name
        var winningHorse = await _betRepository.GetHorseByIdOrNameAsync(result.Winner);
        if (winningHorse == null)
        {
            throw new InvalidOperationException($"Winning horse '{result.Winner}' could not be found in the database.");
        }

        var bets = await _betRepository.GetByRaceIdAsync(raceId);
        var betList = bets.Where(b => b.Status == "Pending").ToList();

        if (betList.Count == 0)
        {
            return; // No bets to process
        }

        foreach (var bet in betList)
        {
            if (bet.HorseId == winningHorse.Id)
            {
                decimal payoutAmount = Math.Round(bet.Amount * bet.Odds, 2);
                bet.Status = "Won";

                var payout = new Payout
                {
                    BetId = bet.Id,
                    Amount = payoutAmount,
                    CreatedAt = DateTime.UtcNow
                };
                await _payoutRepository.AddAsync(payout);

                var wallet = await _walletRepository.GetByUserIdAsync(bet.UserId);
                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = bet.UserId,
                        Balance = 0
                    };
                    await _walletRepository.AddAsync(wallet);
                    await _walletRepository.SaveChangesAsync();
                }

                wallet.Balance += payoutAmount;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = payoutAmount,
                    Type = "BetWon",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(transaction);

                var notification = new Notification
                {
                    UserId = bet.UserId,
                    Message = $"Congratulations! Your bet on '{winningHorse.Name}' in race '{race.Name}' won. Payout of {payoutAmount:N2} has been credited to your wallet. New balance: {wallet.Balance:N2}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
            }
            else
            {
                bet.Status = "Lost";

                var horse = await _betRepository.GetHorseByIdOrNameAsync(bet.HorseId.ToString());
                var horseName = horse?.Name ?? "your chosen horse";

                var notification = new Notification
                {
                    UserId = bet.UserId,
                    Message = $"Your bet on '{horseName}' in race '{race.Name}' lost. Better luck next time!",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
            }
        }

        await _betRepository.SaveChangesAsync();
    }
}
