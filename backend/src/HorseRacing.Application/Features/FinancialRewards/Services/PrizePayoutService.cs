using System;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Domain.Entities.Financials;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class PrizePayoutService : IPrizePayoutService
{
    private readonly IBetRepository _betRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly INotificationRepository _notificationRepository;

    public PrizePayoutService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPrizeRepository prizeRepository,
        INotificationRepository notificationRepository)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _prizeRepository = prizeRepository;
        _notificationRepository = notificationRepository;
    }

    private async Task<Wallet> GetOrCreateWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
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

    public async Task ProcessPrizePayoutAsync(PrizePayoutRequest request)
    {
        var tournament = await _betRepository.GetTournamentByIdAsync(request.TournamentId);
        if (tournament == null)
        {
            throw new ArgumentException($"Tournament with ID {request.TournamentId} not found.");
        }

        // 1. Configure and save First, Second, Third place prizes
        var firstPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 1);
        if (firstPrize == null)
        {
            firstPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 1,
                Amount = request.FirstPlacePrize > 0 ? request.FirstPlacePrize : 10000m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(firstPrize);
        }
        else if (request.FirstPlacePrize > 0)
        {
            firstPrize.Amount = request.FirstPlacePrize;
        }

        var secondPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 2);
        if (secondPrize == null)
        {
            secondPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 2,
                Amount = request.SecondPlacePrize > 0 ? request.SecondPlacePrize : 5000m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(secondPrize);
        }
        else if (request.SecondPlacePrize > 0)
        {
            secondPrize.Amount = request.SecondPlacePrize;
        }

        var thirdPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 3);
        if (thirdPrize == null)
        {
            thirdPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 3,
                Amount = request.ThirdPlacePrize > 0 ? request.ThirdPlacePrize : 2500m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(thirdPrize);
        }
        else if (request.ThirdPlacePrize > 0)
        {
            thirdPrize.Amount = request.ThirdPlacePrize;
        }

        await _prizeRepository.SaveChangesAsync();

        // 2. Try to get finished final race and published result. If not ready, just exit with configurations saved.
        var finalRace = await _betRepository.GetFinalRaceInTournamentAsync(request.TournamentId);
        if (finalRace == null)
        {
            // Prizes saved, but no finished races to process payouts for. Return gracefully.
            return;
        }

        var result = await _betRepository.GetRaceResultAsync(finalRace.RaceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            // Prizes saved, but winner result not published. Return gracefully.
            return;
        }

        // 3. Process payout for the tournament winner (First Place)
        var winningHorse = await _betRepository.GetHorseByIdOrNameAsync(result.Winner);
        if (winningHorse == null)
        {
            throw new InvalidOperationException($"Winning horse '{result.Winner}' from final race results could not be found.");
        }

        var winningEntry = await _betRepository.GetRaceEntryAsync(finalRace.RaceId, (int)winningHorse.HorseId);
        if (winningEntry == null)
        {
            throw new InvalidOperationException($"Could not find the race entry matching horse '{winningHorse.Name}' in final race ID {finalRace.RaceId}.");
        }

        decimal ownerAmount = Math.Round(firstPrize.Amount * (firstPrize.OwnerPercentage / 100m), 2);
        decimal jockeyAmount = Math.Round(firstPrize.Amount * (firstPrize.JockeyPercentage / 100m), 2);

        var ownerWallet = await GetOrCreateWalletAsync(winningHorse.OwnerId);
        ownerWallet.Balance += ownerAmount;

        var ownerTransaction = new WalletTransaction
        {
            WalletId = ownerWallet.WalletId,
            Amount = ownerAmount,
            Type = "PrizePayout",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(ownerTransaction);

        var ownerPayoutRecord = new TournamentPrizePayout
        {
            TournamentId = request.TournamentId,
            UserId = winningHorse.OwnerId,
            Amount = ownerAmount,
            Role = "HorseOwner",
            CreatedAt = DateTime.UtcNow
        };
        await _prizeRepository.AddTournamentPrizePayoutAsync(ownerPayoutRecord);

        var ownerNotification = new Notification
        {
            UserId = winningHorse.OwnerId,
            Message = $"Congratulations! Your horse '{winningHorse.Name}' won the tournament '{tournament.Name}'. You have been awarded the Owner's Prize share of {ownerAmount:N2}. New balance: {ownerWallet.Balance:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(ownerNotification);

        int jockeyUserId = 0;
        if (winningEntry.JockeyId.HasValue)
        {
            if (winningEntry.JockeyProfile == null)
            {
                throw new InvalidOperationException($"Jockey Profile with ID {winningEntry.JockeyId.Value} is not loaded.");
            }
            jockeyUserId = winningEntry.JockeyProfile.UserId;
        }
        var jockeyWallet = await GetOrCreateWalletAsync(jockeyUserId);
        jockeyWallet.Balance += jockeyAmount;

        var jockeyTransaction = new WalletTransaction
        {
            WalletId = jockeyWallet.WalletId,
            Amount = jockeyAmount,
            Type = "PrizePayout",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(jockeyTransaction);

        var jockeyPayoutRecord = new TournamentPrizePayout
        {
            TournamentId = request.TournamentId,
            UserId = jockeyUserId,
            Amount = jockeyAmount,
            Role = "Jockey",
            CreatedAt = DateTime.UtcNow
        };
        await _prizeRepository.AddTournamentPrizePayoutAsync(jockeyPayoutRecord);

        var jockeyNotification = new Notification
        {
            UserId = jockeyUserId,
            Message = $"Congratulations! You won the tournament '{tournament.Name}' riding '{winningHorse.Name}'. You have been awarded the Jockey's Prize share of {jockeyAmount:N2}. New balance: {jockeyWallet.Balance:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(jockeyNotification);

        tournament.Status = "Completed";

        await _prizeRepository.SaveChangesAsync();
    }
}
