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
    private readonly INotificationService _notificationService;

    public BetPayoutService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPayoutRepository payoutRepository,
        INotificationService notificationService)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _payoutRepository = payoutRepository;
        _notificationService = notificationService;
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

        // Get race entries
        var entries = (await _betRepository.GetRaceEntriesWithHorseAsync(raceId)).ToList();

        // Find winner RaceEntry with FinishPosition == 1
        long? winnerRaceEntryId = null;
        var winnerEntry = entries.FirstOrDefault(re => re.FinishPosition == 1);
        if (winnerEntry != null)
        {
            winnerRaceEntryId = winnerEntry.RaceEntryId;
        }

        // Try to resolve the winning horse by ID or by Name
        var winningHorse = await _betRepository.GetHorseByIdOrNameAsync(result.Winner);
        if (winningHorse == null)
        {
            throw new InvalidOperationException($"Winning horse '{result.Winner}' could not be found in the database.");
        }

        if (winnerRaceEntryId == null)
        {
            var entry = entries.FirstOrDefault(re => re.Registration != null && re.Registration.HorseId == winningHorse.HorseId);
            if (entry != null)
            {
                winnerRaceEntryId = entry.RaceEntryId;
            }
        }

        var bets = await _betRepository.GetByRaceIdAsync(raceId);
        var betList = bets.Where(b => b.Status == "Pending").ToList();

        if (betList.Count == 0)
        {
            return; // No bets to process
        }

        foreach (var bet in betList)
        {
            bool isWon = false;
            if (winnerRaceEntryId.HasValue)
            {
                isWon = bet.RaceEntryId == winnerRaceEntryId.Value || (bet.RaceEntryId == null && bet.HorseId == winningHorse.HorseId);
            }
            else
            {
                isWon = bet.HorseId == winningHorse.HorseId;
            }

            if (isWon)
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

                await _notificationService.SendNotificationToUserAsync(
                    bet.UserId,
                    "Kết quả đặt cược",
                    $"Con ngựa '{winningHorse.Name}' của bạn cược đã thắng ở trong top 3! Số tiền nhận thưởng: {payoutAmount:N2}$.",
                    "Bet",
                    referenceId: (int)bet.Id,
                    actionUrl: "/spectator/predictions"
                );
            }
            else
            {
                bet.Status = "Lost";

                var horse = await _betRepository.GetHorseByIdOrNameAsync(bet.HorseId.ToString());
                var horseName = horse?.Name ?? "ngựa đã chọn";

                await _notificationService.SendNotificationToUserAsync(
                    bet.UserId,
                    "Kết quả đặt cược",
                    $"Bạn đã cược con ngựa '{horseName}' nhưng kết quả không đúng.",
                    "Bet",
                    referenceId: (int)bet.Id,
                    actionUrl: "/spectator/predictions"
                );
            }
        }

        await _betRepository.SaveChangesAsync();
    }
}
