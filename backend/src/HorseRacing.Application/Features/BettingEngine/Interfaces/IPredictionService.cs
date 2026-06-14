using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IPredictionService
{
    Task<PredictionResponse> CreatePredictionAsync(int userId, CreatePredictionRequest request);
    Task<IEnumerable<PredictionResponse>> GetMyPredictionsAsync(int userId);
    Task<IEnumerable<PredictionResponse>> GetPredictionsByRaceAsync(long raceId);
    Task EvaluatePredictionsAsync(long raceId);
}
