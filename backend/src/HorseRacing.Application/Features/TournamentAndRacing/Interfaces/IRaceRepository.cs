using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface IRaceRepository
{
    Task AddAsync(Race race);
    Task SaveChangesAsync();
    Task<List<Race>> GetPublicRaceScheduleAsync();
    Task<Round?> GetRoundByIdAsync(long roundId);
    Task<Race?> GetByIdWithDetailsAsync(long raceId);
    Task<Registration?> GetRegistrationWithHorseAsync(long registrationId);
    Task<List<RaceEntry>> GetRaceEntriesAsync(long raceId);
    Task<HashSet<long>> GetRaceIdsWithHealthIssuesAsync(IEnumerable<long> raceIds);
    Task<bool> HasActiveJockeyContractAsync(long tournamentId, long horseId, int jockeyId);
    Task<bool> HasHorseScheduleConflictAsync(long horseId, long excludedRaceId, DateTime raceDate);
    Task<bool> HasJockeyScheduleConflictAsync(int jockeyId, long excludedRaceId, DateTime raceDate);
    Task<bool> HasFinancialOrResultDataAsync(long raceId);
    Task<(int JockeyProfileId, string JockeyName)?> GetActiveJockeyForHorseAsync(long tournamentId, long horseId);
    Task AddRaceEntryAsync(RaceEntry raceEntry);
    Task DeleteRaceAsync(long raceId);
}
