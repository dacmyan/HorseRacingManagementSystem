using System;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

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

        var finalRace = await _betRepository.GetFinalRaceInTournamentAsync(request.TournamentId);
        if (finalRace == null)
        {
            throw new InvalidOperationException($"No finished races found for tournament ID {request.TournamentId}. Cannot distribute prizes without results.");
        }

        var result = await _betRepository.GetRaceResultAsync(finalRace.RaceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            throw new InvalidOperationException($"No published results or winner found for the final race (ID {finalRace.RaceId}) of tournament ID {request.TournamentId}.");
        }

        var winningHorse = await _betRepository.GetHorseByIdOrNameAsync(result.Winner);
        if (winningHorse == null)
        {
            throw new InvalidOperationException($"Winning horse '{result.Winner}' from final race results could not be found.");
        }

        var winningEntry = await _betRepository.GetRaceEntryAsync(finalRace.RaceId, winningHorse.Id);
        if (winningEntry == null)
        {
            throw new InvalidOperationException($"Could not find the race entry matching horse '{winningHorse.Name}' in final race ID {finalRace.RaceId}.");
        }

        var firstPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 1);
        if (firstPrize == null)
        {
            firstPrize = new Prize
            {
                TournamentId = request.TournamentId,
                Rank = 1,
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

        await _prizeRepository.SaveChangesAsync();

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

        var jockeyWallet = await GetOrCreateWalletAsync(winningEntry.JockeyId);
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
            UserId = winningEntry.JockeyId,
            Amount = jockeyAmount,
            Role = "Jockey",
            CreatedAt = DateTime.UtcNow
        };
        await _prizeRepository.AddTournamentPrizePayoutAsync(jockeyPayoutRecord);

        var jockeyNotification = new Notification
        {
            UserId = winningEntry.JockeyId,
            Message = $"Congratulations! You won the tournament '{tournament.Name}' riding '{winningHorse.Name}'. You have been awarded the Jockey's Prize share of {jockeyAmount:N2}. New balance: {jockeyWallet.Balance:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(jockeyNotification);

        tournament.Status = "Completed";

        await _prizeRepository.SaveChangesAsync();
    }
}
