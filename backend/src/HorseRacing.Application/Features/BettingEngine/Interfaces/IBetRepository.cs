using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.BettingEngine.Interfaces;

public interface IBetRepository
{
    // Bet operations
    Task<Bet?> GetByIdAsync(int id);
    Task<IEnumerable<Bet>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Bet>> GetByRaceIdAsync(long raceId);
    Task AddAsync(Bet bet);
    Task SaveChangesAsync();

    // Race queries
    Task<Race?> GetRaceByIdAsync(long raceId);
    Task<bool> IsHorseInRaceAsync(long raceId, int horseId);
    Task<RaceResult?> GetRaceResultAsync(long raceId);
    Task<RaceEntry?> GetRaceEntryAsync(long raceId, int horseId);
    Task<Horse?> GetHorseByIdOrNameAsync(string identifier);
    Task<Tournament?> GetTournamentByIdAsync(long tournamentId);
    Task<Race?> GetFinalRaceInTournamentAsync(long tournamentId);
    Task<RaceEntry?> GetRaceEntryByIdAsync(long raceEntryId);
    Task<IEnumerable<RaceEntry>> GetRaceEntriesWithHorseAsync(long raceId);



    // Rankings queries
    Task<IEnumerable<JockeyProfile>> GetJockeyRankingsAsync();
    Task<IEnumerable<Horse>> GetHorsesWithWinnersAsync();
    Task<IEnumerable<RaceResult>> GetFinishedRaceResultsAsync();

    Task<decimal> GetTotalBetsForRaceAsync(long raceId);
    Task<decimal> GetTotalPayoutsForRaceAsync(long raceId);
}
