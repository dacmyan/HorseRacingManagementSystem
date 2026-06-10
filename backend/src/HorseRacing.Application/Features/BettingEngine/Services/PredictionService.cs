using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.BettingEngine.Services;

public class PredictionService : IPredictionService
{
    private readonly IBetRepository _betRepository;

    public PredictionService(IBetRepository betRepository)
    {
        _betRepository = betRepository;
    }

    public async Task<PredictionStatsResponse> PlacePredictionAsync(int userId, PredictionManagementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PredictedWinner))
        {
            throw new ArgumentException("Predicted winner cannot be empty.");
        }

        var race = await _betRepository.GetRaceByIdAsync(request.RaceId);
        if (race == null)
        {
            throw new ArgumentException($"Race with ID {request.RaceId} not found.");
        }

        if (!race.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot place prediction. Race status is '{race.Status}'. Predictions only allowed for 'Scheduled' races.");
        }

        var existingPrediction = await _betRepository.GetPredictionAsync(request.RaceId, userId);

        if (existingPrediction != null)
        {
            existingPrediction.PredictedWinner = request.PredictedWinner;
        }
        else
        {
            var newPrediction = new Prediction
            {
                RaceId = request.RaceId,
                UserId = userId,
                PredictedWinner = request.PredictedWinner
            };
            await _betRepository.AddPredictionAsync(newPrediction);
        }

        await _betRepository.SaveChangesAsync();

        return await GetPredictionStatsAsync(request.RaceId);
    }

    public async Task<PredictionStatsResponse> GetPredictionStatsAsync(int raceId)
    {
        var race = await _betRepository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new ArgumentException($"Race with ID {raceId} not found.");
        }

        var predictions = await _betRepository.GetPredictionsByRaceIdAsync(raceId);
        var predictionList = predictions.ToList();

        var total = predictionList.Count;
        var details = new List<HorsePredictionStat>();

        if (total > 0)
        {
            var grouped = predictionList
                .GroupBy(p => p.PredictedWinner)
                .Select(g => new HorsePredictionStat
                {
                    PredictedWinner = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((double)g.Count() / total * 100, 2)
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            details.AddRange(grouped);
        }

        return new PredictionStatsResponse
        {
            RaceId = raceId,
            RaceName = race.Name,
            TotalPredictions = total,
            Details = details
        };
    }
}
