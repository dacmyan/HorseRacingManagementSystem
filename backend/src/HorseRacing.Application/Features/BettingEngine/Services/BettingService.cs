using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.BettingEngine.Services;

public class BettingService : IBettingService
{
    private readonly IBetRepository _betRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly INotificationRepository _notificationRepository;

    public BettingService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        INotificationRepository notificationRepository)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<decimal> CalculateCurrentOddsAsync(long raceId, int horseId)
    {
        var bets = await _betRepository.GetByRaceIdAsync(raceId);
        var activeBets = bets.Where(b => b.Status != "Refunded").ToList();

        var totalRaceBets = activeBets.Sum(b => b.Amount);
        var totalHorseBets = activeBets.Where(b => b.HorseId == horseId).Sum(b => b.Amount);

        if (totalRaceBets == 0)
        {
            return 2.0m;
        }

        if (totalHorseBets == 0)
        {
            var tempOdds = (totalRaceBets * 0.9m) / 100m;
            return Math.Max(tempOdds, 3.5m);
        }

        var odds = (totalRaceBets * 0.9m) / totalHorseBets;
        return Math.Max(odds, 1.1m);
    }

    public async Task<BetTicketResponse> PlaceBetAsync(int userId, PlaceBetRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Bet amount must be greater than zero.");
        }

        var race = await _betRepository.GetRaceByIdAsync(request.RaceId);
        if (race == null)
        {
            throw new ArgumentException($"Race with ID {request.RaceId} not found.");
        }
        if (!race.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot place bet. Race status is '{race.Status}'. Only 'Scheduled' races accept bets.");
        }

        var isHorseInRace = await _betRepository.IsHorseInRaceAsync(request.RaceId, request.HorseId);
        if (!isHorseInRace)
        {
            throw new ArgumentException($"Horse with ID {request.HorseId} is not registered in this race.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            throw new InvalidOperationException("User wallet not found. Please deposit funds first.");
        }
        if (wallet.Balance < request.Amount)
        {
            throw new InvalidOperationException($"Insufficient wallet balance. Current: {wallet.Balance:N2}, Required: {request.Amount:N2}.");
        }

        var currentOdds = await CalculateCurrentOddsAsync(request.RaceId, request.HorseId);

        wallet.Balance -= request.Amount;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.WalletId,
            Amount = -request.Amount,
            Type = "BetPlaced",
            CreatedAt = DateTime.UtcNow
        };
        await _transactionRepository.AddAsync(transaction);

        var bet = new Bet
        {
            UserId = userId,
            RaceId = request.RaceId,
            HorseId = request.HorseId,
            Amount = request.Amount,
            Odds = currentOdds,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        await _betRepository.AddAsync(bet);
        await _betRepository.SaveChangesAsync();

        var horse = await _betRepository.GetHorseByIdOrNameAsync(request.HorseId.ToString());
        var horseName = horse?.Name ?? "Unknown Horse";

        var notification = new Notification
        {
            UserId = userId,
            Message = $"You placed a bet of {request.Amount:N2} on '{horseName}' in race '{race.Name}' at odds of {currentOdds:N2}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return new BetTicketResponse
        {
            Id = bet.Id,
            UserId = bet.UserId,
            RaceId = bet.RaceId,
            RaceName = race.Name,
            HorseId = bet.HorseId,
            HorseName = horseName,
            Amount = bet.Amount,
            Odds = bet.Odds,
            Status = bet.Status,
            CreatedAt = bet.CreatedAt
        };
    }

    public async Task<IEnumerable<BetTicketResponse>> GetMyBetsAsync(int userId)
    {
        var bets = await _betRepository.GetByUserIdAsync(userId);

        return bets.Select(b => new BetTicketResponse
        {
            Id = b.Id,
            UserId = b.UserId,
            RaceId = b.RaceId,
            RaceName = b.Race?.Name ?? "Unknown Race",
            HorseId = b.HorseId,
            HorseName = b.Horse?.Name ?? "Unknown Horse",
            Amount = b.Amount,
            Odds = b.Odds,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        });
    }
}
