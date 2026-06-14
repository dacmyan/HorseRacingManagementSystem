using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(int id);
    Task<Prediction?> GetByUserIdAndRaceIdAsync(int userId, long raceId);
    Task<IEnumerable<Prediction>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Prediction>> GetByRaceIdAsync(long raceId);
    Task AddAsync(Prediction prediction);
    Task SaveChangesAsync();
    
    // Validation helpers
    Task<bool> IsSpectatorAsync(int userId);
    Task<bool> RaceExistsAsync(long raceId);
    Task<bool> RaceEntryExistsAsync(long raceId, long raceEntryId);
    Task<string?> GetRaceStatusAsync(long raceId);
}
