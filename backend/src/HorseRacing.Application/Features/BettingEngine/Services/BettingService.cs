using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

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

        var entry = await _betRepository.GetRaceEntryByIdAsync(request.RaceEntryId);

        if (entry == null)
        {
            throw new ArgumentException($"Race entry with ID {request.RaceEntryId} not found.");
        }

        var race = entry.Race;
        if (race == null)
        {
            throw new ArgumentException("Race details not found for this entry.");
        }

        var horse = entry.Registration?.Horse;
        if (horse == null)
        {
            throw new ArgumentException("Horse details not found for this entry.");
        }

        var invalidTournamentStatuses = new[] { "finished", "completed", "cancelled", "ended" };
        var tournamentStatus = race.Round?.Tournament?.Status?.ToLower() ?? "";
        if (invalidTournamentStatuses.Contains(tournamentStatus))
        {
            throw new ArgumentException("Cannot place bet because tournament has ended.");
        }

        var validRaceStatuses = new[] { "upcoming", "scheduled", "ongoing", "running" };
        var raceStatus = race.Status?.ToLower() ?? "";
        if (!validRaceStatuses.Contains(raceStatus))
        {
            throw new ArgumentException("Betting is only allowed for upcoming or ongoing races.");
        }

        // Check if race results already exist
        var result = await _betRepository.GetRaceResultAsync(race.RaceId);
        if (result != null)
        {
            throw new ArgumentException("Cannot place bet because race results have already been published.");
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

        // Odds must be taken from RaceEntry.CurrentOdds at the moment of betting
        var currentOdds = entry.CurrentOdds ?? 2.0m;

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
            RaceId = race.RaceId,
            HorseId = horse.HorseId,
            RaceEntryId = entry.RaceEntryId,
            Amount = request.Amount,
            Odds = currentOdds,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        await _betRepository.AddAsync(bet);

        // Save wallet changes
        await _walletRepository.SaveChangesAsync();

        var notification = new Notification
        {
            UserId = userId,
            Message = $"You placed a bet of {request.Amount:N2} on '{horse.Name}' in race '{race.Name}' at odds of {currentOdds:N2}.",
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
            RaceName = race.Name ?? "Unknown Race",
            HorseId = (int)bet.HorseId,
            HorseName = horse.Name,
            RaceEntryId = bet.RaceEntryId,
            Amount = bet.Amount,
            Odds = bet.Odds,
            Status = bet.Status,
            CreatedAt = bet.CreatedAt,
            PotentialPayout = bet.Amount * bet.Odds,
            ActualPayout = null,
            PayoutStatus = "Pending"
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
            HorseId = (int)b.HorseId,
            HorseName = b.Horse?.Name ?? "Unknown Horse",
            RaceEntryId = b.RaceEntryId,
            Amount = b.Amount,
            Odds = b.Odds,
            Status = b.Status,
            CreatedAt = b.CreatedAt,
            PotentialPayout = b.Amount * b.Odds,
            ActualPayout = b.Status == "Won" || b.Status == "PaidOut" ? b.Amount * b.Odds : null,
            PayoutStatus = b.Status switch
            {
                "Won" => "Won",
                "PaidOut" => "PaidOut",
                "Lost" => "Lost",
                _ => "Pending"
            }
        });
    }

    public async Task RecalculateRaceOddsAsync(long raceId)
    {
        var entries = (await _betRepository.GetRaceEntriesWithHorseAsync(raceId)).ToList();

        if (entries.Count == 0) return;

        var scores = new List<(RaceEntry entry, decimal score)>();

        foreach (var entry in entries)
        {
            var horse = entry.Registration?.Horse;
            if (horse == null) continue;

            var averageTime = horse.AverageTime;
            if (averageTime == null || averageTime <= 0)
            {
                averageTime = 70;
            }

            var recentAverageTime = horse.RecentAverageTime;
            if (recentAverageTime == null || recentAverageTime <= 0)
            {
                recentAverageTime = averageTime;
            }

            var winRate = horse.WinRate;
            if (winRate == null)
            {
                winRate = 0.05m;
            }

            if (winRate > 1)
            {
                winRate = winRate / 100m;
            }

            var averageTimeScore = 1m / (averageTime.Value * averageTime.Value);
            var recentTimeScore = 1m / (recentAverageTime.Value * recentAverageTime.Value);
            var winRateScore = winRate.Value;

            var horseScore =
                averageTimeScore * 0.5m
                + recentTimeScore * 0.3m
                + winRateScore * 0.2m;

            scores.Add((entry, horseScore));
        }

        var totalScore = scores.Sum(x => x.score);

        if (totalScore <= 0)
        {
            var equalProbability = 1m / entries.Count;

            foreach (var entry in entries)
            {
                entry.WinningProbability = Math.Round(equalProbability * 100m, 2);
                entry.CurrentOdds = Math.Round((1m / equalProbability) * 0.9m, 2);
            }
        }
        else
        {
            foreach (var item in scores)
            {
                var winProbability = item.score / totalScore;
                var winPercentage = winProbability * 100m;
                var odds = 1m / winProbability;
                var finalOdds = odds * 0.9m;

                item.entry.WinningProbability = Math.Round(winPercentage, 2);
                item.entry.CurrentOdds = Math.Round(finalOdds, 2);
            }
        }

        await _betRepository.SaveChangesAsync();
    }

    public async Task<RaceBettingInfoResponse> GetRaceBettingInfoAsync(int userId, long raceId)
    {
        var race = await _betRepository.GetRaceByIdAsync(raceId);

        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} not found.");
        }

        var entries = (await _betRepository.GetRaceEntriesWithHorseAsync(raceId)).ToList();

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        var balance = wallet?.Balance ?? 0;

        var hasResult = (await _betRepository.GetRaceResultAsync(raceId)) != null;

        var invalidTournamentStatuses = new[] { "finished", "completed", "cancelled", "ended" };
        var tournamentStatus = race.Round?.Tournament?.Status?.ToLower() ?? "";
        var isTournamentEnded = invalidTournamentStatuses.Contains(tournamentStatus);

        var validRaceStatuses = new[] { "upcoming", "scheduled", "ongoing", "running" };
        var raceStatus = race.Status?.ToLower() ?? "";
        var isRaceActive = validRaceStatuses.Contains(raceStatus);

        bool canBet = isRaceActive && !isTournamentEnded && !hasResult;

        var entriesDto = entries.Select(e => new RaceEntryBettingDto
        {
            RaceEntryId = e.RaceEntryId,
            LaneNo = e.LaneNo,
            HorseId = e.Registration?.HorseId ?? 0,
            HorseName = e.Registration?.Horse?.Name ?? "Unknown Horse",
            JockeyName = e.JockeyProfile?.User?.FullName ?? "Unknown Jockey",
            AverageTime = e.Registration?.Horse?.AverageTime ?? 70m,
            RecentAverageTime = e.Registration?.Horse?.RecentAverageTime ?? (e.Registration?.Horse?.AverageTime ?? 70m),
            WinRate = e.Registration?.Horse?.WinRate ?? 0.05m,
            WinningProbability = e.WinningProbability ?? 0m,
            CurrentOdds = e.CurrentOdds ?? 2.0m
        }).ToList();

        return new RaceBettingInfoResponse
        {
            RaceId = race.RaceId,
            RaceName = race.Name ?? string.Empty,
            RaceStatus = race.Status,
            TournamentStatus = race.Round?.Tournament?.Status ?? string.Empty,
            CanBet = canBet,
            Entries = entriesDto
        };
    }
}
