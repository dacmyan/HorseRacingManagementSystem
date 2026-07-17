using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.BettingEngine.Services;

public class PredictionService : IPredictionService
{
    private readonly IPredictionRepository _predictionRepository;
    private readonly IResultRepository _resultRepository;
    private readonly INotificationService _notificationService;

    public PredictionService(
        IPredictionRepository predictionRepository,
        IResultRepository resultRepository,
        INotificationService notificationService)
    {
        _predictionRepository = predictionRepository;
        _resultRepository = resultRepository;
        _notificationService = notificationService;
    }

    public async Task<PredictionResponse> CreatePredictionAsync(int userId, CreatePredictionRequest request)
    {
        // 1. Check if user is spectator
        var isSpectator = await _predictionRepository.IsSpectatorAsync(userId);
        if (!isSpectator)
        {
            throw new InvalidOperationException("Only Spectator users are allowed to make predictions.");
        }

        // 2. Check if race exists
        var raceStatus = await _predictionRepository.GetRaceStatusAsync(request.RaceId);
        if (raceStatus == null)
        {
            throw new ArgumentException($"Race with ID {request.RaceId} not found.");
        }

        // 3. Check if race entry exists
        var entryExists = await _predictionRepository.RaceEntryExistsAsync(request.RaceId, request.RaceEntryId);
        if (!entryExists)
        {
            throw new ArgumentException($"Race entry with ID {request.RaceEntryId} is not registered in this race.");
        }

        // 4. Check status is Scheduled
        if (!raceStatus.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot make prediction. Race status is '{raceStatus}'. Predictions are only allowed for 'Scheduled' races.");
        }

        // 5. Check duplicate prediction
        var existing = await _predictionRepository.GetByUserIdAndRaceIdAsync(userId, request.RaceId);
        if (existing != null)
        {
            throw new InvalidOperationException($"You have already made a prediction for race ID {request.RaceId}.");
        }

        // 6. Create prediction
        var prediction = new Prediction
        {
            UserId = userId,
            RaceId = request.RaceId,
            RaceEntryId = request.RaceEntryId,
            Status = "Pending",
            IsCorrect = null,
            Point = 0,
            PredictedAt = DateTime.UtcNow
        };

        await _predictionRepository.AddAsync(prediction);
        await _predictionRepository.SaveChangesAsync();

        // Send notification for prediction submission
        var race = await _resultRepository.GetRaceByIdAsync(request.RaceId);
        await _notificationService.SendNotificationToUserAsync(
            userId,
            "Prediction Submitted Successfully",
            $"You successfully submitted a prediction for race '{race?.Name ?? request.RaceId.ToString()}'.",
            "Race",
            referenceId: (int)request.RaceId,
            actionUrl: $"/spectator/races/{request.RaceId}"
        );

        return new PredictionResponse
        {
            PredictionId = prediction.PredictionId,
            RaceId = prediction.RaceId,
            RaceEntryId = prediction.RaceEntryId,
            Status = prediction.Status,
            IsCorrect = prediction.IsCorrect,
            Point = prediction.Point,
            PredictedAt = prediction.PredictedAt
        };
    }

    public async Task<IEnumerable<PredictionResponse>> GetMyPredictionsAsync(int userId)
    {
        var predictions = await _predictionRepository.GetByUserIdAsync(userId);
        return predictions.Select(p => new PredictionResponse
        {
            PredictionId = p.PredictionId,
            RaceId = p.RaceId,
            RaceEntryId = p.RaceEntryId,
            Status = p.Status,
            IsCorrect = p.IsCorrect,
            Point = p.Point,
            PredictedAt = p.PredictedAt
        });
    }

    public async Task<IEnumerable<PredictionResponse>> GetPredictionsByRaceAsync(long raceId)
    {
        var predictions = await _predictionRepository.GetByRaceIdAsync(raceId);
        return predictions.Select(p => new PredictionResponse
        {
            PredictionId = p.PredictionId,
            RaceId = p.RaceId,
            RaceEntryId = p.RaceEntryId,
            Status = p.Status,
            IsCorrect = p.IsCorrect,
            Point = p.Point,
            PredictedAt = p.PredictedAt
        });
    }

    public async Task EvaluatePredictionsAsync(long raceId)
    {
        var result = await _resultRepository.GetResultByRaceIdAsync(raceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            throw new InvalidOperationException($"No published result or winner found for race ID {raceId}.");
        }

        var winningHorse = await _resultRepository.GetHorseByIdOrNameAsync(result.Winner);
        if (winningHorse == null)
        {
            throw new InvalidOperationException($"Winning horse '{result.Winner}' not found.");
        }

        var winningEntry = await _resultRepository.GetRaceEntryByHorseIdAsync(raceId, winningHorse.HorseId);
        if (winningEntry == null)
        {
            throw new InvalidOperationException($"Winning race entry for horse '{winningHorse.Name}' in race ID {raceId} not found.");
        }

        var race = await _resultRepository.GetRaceByIdAsync(raceId);
        var raceName = race?.Name ?? $"Race #{raceId}";

        var predictions = await _predictionRepository.GetByRaceIdAsync(raceId);
        var pendingPredictions = predictions.Where(p => p.Status == "Pending").ToList();

        if (pendingPredictions.Count == 0)
        {
            return;
        }

        foreach (var prediction in pendingPredictions)
        {
            if (prediction.RaceEntryId == winningEntry.RaceEntryId)
            {
                prediction.IsCorrect = true;
                prediction.Point = 1;
                prediction.Status = "Evaluated";

                await _notificationService.SendNotificationToUserAsync(
                    prediction.UserId,
                    "Correct Prediction!",
                    $"Your prediction for race '{raceName}' was correct! You received +1 points.",
                    "Race",
                    referenceId: (int)raceId,
                    actionUrl: $"/spectator/races/{raceId}"
                );
            }
            else
            {
                prediction.IsCorrect = false;
                prediction.Point = 0;
                prediction.Status = "Evaluated";

                await _notificationService.SendNotificationToUserAsync(
                    prediction.UserId,
                    "Incorrect Prediction",
                    $"Your prediction for race '{raceName}' was incorrect. Good luck next time!",
                    "Race",
                    referenceId: (int)raceId,
                    actionUrl: $"/spectator/races/{raceId}"
                );
            }
        }

        await _predictionRepository.SaveChangesAsync();
    }
}
