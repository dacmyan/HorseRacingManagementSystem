using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IBetRepository
{
    // Bet operations
    Task<Bet?> GetByIdAsync(int id);
    Task<IEnumerable<Bet>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Bet>> GetByRaceIdAsync(int raceId);
    Task AddAsync(Bet bet);
    Task SaveChangesAsync();

    // Race queries
    Task<Race?> GetRaceByIdAsync(int raceId);
    Task<bool> IsHorseInRaceAsync(int raceId, int horseId);
    Task<RaceResult?> GetRaceResultAsync(int raceId);
    Task<RaceEntry?> GetRaceEntryAsync(int raceId, int horseId);
    Task<Horse?> GetHorseByIdOrNameAsync(string identifier);
    Task<Tournament?> GetTournamentByIdAsync(int tournamentId);
    Task<Race?> GetFinalRaceInTournamentAsync(int tournamentId);

    // Prediction queries
    Task<Prediction?> GetPredictionAsync(int raceId, int userId);
    Task<IEnumerable<Prediction>> GetPredictionsByRaceIdAsync(int raceId);
    Task AddPredictionAsync(Prediction prediction);

    // Rankings queries
    Task<IEnumerable<JockeyProfile>> GetJockeyRankingsAsync();
    Task<IEnumerable<Horse>> GetHorsesWithWinnersAsync();
    Task<IEnumerable<RaceResult>> GetFinishedRaceResultsAsync();
}
