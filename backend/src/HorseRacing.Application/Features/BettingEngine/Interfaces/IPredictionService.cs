using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IPredictionService
{
    Task<PredictionStatsResponse> PlacePredictionAsync(int userId, PredictionManagementRequest request);
    Task<PredictionStatsResponse> GetPredictionStatsAsync(int raceId);
}
