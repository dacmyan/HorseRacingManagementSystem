using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.DTOs;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IBettingService
{
    Task<BetTicketResponse> PlaceBetAsync(int userId, PlaceBetRequest request);
    Task<IEnumerable<BetTicketResponse>> GetMyBetsAsync(int userId);
    Task<decimal> CalculateCurrentOddsAsync(long raceId, int horseId);
}
