using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using System.Linq;

using HorseRacing.Application.Features.Notifications.Interfaces;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RaceResultService : IRaceResultService
{
    private readonly IResultRepository _repository;
    private readonly IBetPayoutService _betPayoutService;
    private readonly IPredictionService _predictionService;
    private readonly INotificationService _notificationService;
    private readonly IPrizePayoutService _prizePayoutService;

    public RaceResultService(
        IResultRepository repository,
        IBetPayoutService betPayoutService,
        IPredictionService predictionService,
        INotificationService notificationService,
        IPrizePayoutService prizePayoutService)
    {
        _repository = repository;
        _betPayoutService = betPayoutService;
        _predictionService = predictionService;
        _notificationService = notificationService;
        _prizePayoutService = prizePayoutService;
    }

    public async Task<RaceResultResponse> SubmitResultAsync(SubmitRaceResultRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 1. Validate race existence
        var race = await _repository.GetRaceByIdAsync(request.RaceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {request.RaceId} was not found.");
        }

        // 2. Validate referee assignment if RefereeId is provided
        if (request.RefereeId.HasValue)
        {
            var assignment = await _repository.GetAssignmentAsync(request.RaceId, request.RefereeId.Value);
            if (assignment == null)
            {
                throw new InvalidOperationException("The referee is not assigned to this race.");
            }
        }

        // 3. Validate Winner parameter
        if (string.IsNullOrWhiteSpace(request.Winner))
        {
            throw new ArgumentException("Winner identifier cannot be empty.", nameof(request.Winner));
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(request.Winner);
        if (horse == null)
        {
            throw new KeyNotFoundException($"Winner horse '{request.Winner}' was not found.");
        }

        // 4. Validate that the horse is entered in the race
        var entry = await _repository.GetRaceEntryByHorseIdAsync(request.RaceId, horse.HorseId);
        if (entry == null)
        {
            throw new ArgumentException($"Horse '{horse.Name}' is not entered in race with ID {request.RaceId}.");
        }

        // 5. Prevent duplicate result submission for the same race
        var existingResult = await _repository.GetResultByRaceIdAsync(request.RaceId);
        if (existingResult != null)
        {
            throw new InvalidOperationException($"A result has already been submitted for race with ID {request.RaceId}.");
        }

        // Generate or save times and positions for all entries in that race if they exist
        var entries = (await _repository.GetRaceEntriesAsync(request.RaceId))?.ToList() ?? new List<RaceEntry>();
        if (entries.Any())
        {
            if (request.Entries != null && request.Entries.Any())
            {
                foreach (var manualEntry in request.Entries)
                {
                    var match = entries.FirstOrDefault(re => re.RaceEntryId == manualEntry.RaceEntryId);
                    if (match != null)
                    {
                        match.FinishPosition = manualEntry.FinishPosition;
                        match.FinishTime = manualEntry.FinishTime;
                    }
                }
                await _repository.SaveChangesAsync();
            }
            else
            {
                var winnerEntry = entries.FirstOrDefault(re => re.Registration?.HorseId == horse.HorseId);
                if (winnerEntry == null)
                {
                    throw new ArgumentException($"Horse '{horse.Name}' is not entered in race with ID {request.RaceId}.");
                }

                var random = new Random();
                decimal winnerTime = Math.Round(55m + (decimal)random.NextDouble() * 10m, 2);

                winnerEntry.FinishPosition = 1;
                winnerEntry.FinishTime = winnerTime;

                int position = 2;
                foreach (var entryItem in entries.Where(re => re.RaceEntryId != winnerEntry.RaceEntryId))
                {
                    entryItem.FinishPosition = position++;
                    entryItem.FinishTime = Math.Round(winnerTime + (decimal)(random.NextDouble() * 3.0 + 0.5), 2);
                }
                await _repository.SaveChangesAsync();
            }
        }

        // 6. Save result (using only actual properties from DB/entity)
        var result = new RaceResult
        {
            RaceId = request.RaceId,
            Winner = request.Winner
        };

        race.Status = "Completed";

        await _repository.AddResultAsync(result);
        await _repository.SaveChangesAsync();

        return new RaceResultResponse
        {
            Id = result.Id,
            RaceId = result.RaceId,
            RaceName = race.Name ?? string.Empty,
            Winner = result.Winner,
            RaceEntryId = entry.RaceEntryId,
            HorseId = horse.HorseId,
            HorseName = horse.Name,
            JockeyId = entry.JockeyId,
            JockeyName = entry.JockeyProfile?.User?.FullName ?? string.Empty,
            Status = race.Status,
            ResultRecordedAt = result.ResultRecordedAt,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task<RaceResultResponse> PublishResultAsync(long raceId)
    {
        // 1. Validate race existence
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        // 2. Validate result presence
        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            throw new InvalidOperationException($"No results found for race with ID {raceId}. Cannot publish.");
        }

        // 3. Update race status to "Finished"
        bool wasFinished = race.Status.Equals("Finished", StringComparison.OrdinalIgnoreCase);
        race.Status = "Finished";

        if (!wasFinished)
        {
            // Fetch all entries to update horse stats
            var entries = (await _repository.GetRaceEntriesAsync(raceId))?.ToList() ?? new List<RaceEntry>();

            foreach (var entryItem in entries)
            {
                var horseId = entryItem.Registration?.HorseId;
                if (horseId == null) continue;

                await _repository.UpdateHorseStatsAsync(horseId.Value);
            }
        }
        await _repository.SaveChangesAsync();

        if (!wasFinished)
        {
            try
            {
                await _betPayoutService.ProcessPayoutAsync(raceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BetPayout Error] Failed to process auto payout for race {raceId}: {ex.Message}");
            }

            try
            {
                await _predictionService.EvaluatePredictionsAsync(raceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Prediction Error] Failed to evaluate predictions for race {raceId}: {ex.Message}");
            }

            // Auto Prize Payout for Final Round Race
            if (race.Round != null && race.Round.RoundNumber == 2)
            {
                try
                {
                    var payoutRequest = new FinancialRewards.DTOs.PrizePayoutRequest
                    {
                        TournamentId = (int)race.Round.TournamentId,
                        FirstPlacePrize = 0m,
                        SecondPlacePrize = 0m,
                        ThirdPlacePrize = 0m
                    };
                    await _prizePayoutService.ProcessPrizePayoutAsync(payoutRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PrizePayout Error] Failed to process auto prize payout: {ex.Message}");
                }
            }
        }

        // 4. Resolve winner details
        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        try
        {
            await _notificationService.BroadcastNotificationAsync(
                "Kết quả Race đã có",
                $"Kết quả cuộc đua '{race.Name}' đã được công bố. Nhấp để xem kết quả chi tiết.",
                "Race",
                referenceId: (int)race.RaceId,
                actionUrl: $"/spectator/races/{race.RaceId}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Notification Error] Failed to broadcast race result publication: {ex.Message}");
        }

        return new RaceResultResponse
        {
            Id = result.Id,
            RaceId = result.RaceId,
            RaceName = race.Name ?? string.Empty,
            Winner = result.Winner,
            RaceEntryId = entry?.RaceEntryId,
            HorseId = horse?.HorseId,
            HorseName = horse?.Name,
            JockeyId = entry?.JockeyId,
            JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
            Status = race.Status,
            ResultRecordedAt = result.ResultRecordedAt,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task<List<RaceResultResponse>?> GetResultsByRaceIdAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            return null;
        }

        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null)
        {
            return new List<RaceResultResponse>();
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        return new List<RaceResultResponse>
        {
            new RaceResultResponse
            {
                Id = result.Id,
                RaceId = result.RaceId,
                RaceName = race.Name ?? string.Empty,
                Winner = result.Winner,
                RaceEntryId = entry?.RaceEntryId,
                HorseId = horse?.HorseId,
                HorseName = horse?.Name,
                JockeyId = entry?.JockeyId,
                JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
                Status = race.Status,
                ResultRecordedAt = result.ResultRecordedAt,
                CreatedAt = result.CreatedAt
            }
        };
    }

    public async Task<List<RaceResultResponse>?> GetPublicResultsByRaceIdAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            return null;
        }

        // Public should only see published results (Status == "Finished")
        if (race.Status != "Finished")
        {
            return new List<RaceResultResponse>();
        }

        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null)
        {
            return new List<RaceResultResponse>();
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        return new List<RaceResultResponse>
        {
            new RaceResultResponse
            {
                Id = result.Id,
                RaceId = result.RaceId,
                RaceName = race.Name ?? string.Empty,
                Winner = result.Winner,
                RaceEntryId = entry?.RaceEntryId,
                HorseId = horse?.HorseId,
                HorseName = horse?.Name,
                JockeyId = entry?.JockeyId,
                JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
                Status = race.Status,
                ResultRecordedAt = result.ResultRecordedAt,
                CreatedAt = result.CreatedAt
            }
        };
    }
}
