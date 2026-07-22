using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface ITournamentRepository
{
    Task AddAsync(Tournament tournament);
    void Update(Tournament tournament);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(long tournamentId);
    Task<bool> NameExistsAsync(string name, long? excludeTournamentId = null);
    Task<Tournament?> GetByIdAsync(long tournamentId);
    Task<Tournament?> GetByIdWithRoundsAsync(long tournamentId);
    Task<List<Tournament>> GetAllAsync();
    Task<List<HorseRacing.Domain.Entities.Registration>> GetApprovedRegistrationsAsync(long tournamentId);
    Task AddRacesAsync(IEnumerable<HorseRacing.Domain.Entities.Tournaments.Race> races);
    Task AddRaceEntriesAsync(IEnumerable<HorseRacing.Domain.Entities.RaceEntry> entries);
    Task<List<Race>> GetRacesByRoundIdAsync(long roundId);
    Task<List<HorseRacing.Domain.Entities.RaceEntry>> GetRaceEntriesByRaceIdAsync(long raceId);
    Task<Dictionary<long, int>> GetActiveJockeyProfileIdsByHorseAsync(long tournamentId, IEnumerable<long> horseIds);
    Task<List<HorseRacing.Domain.Entities.Registration>> GetTopHorsesFromPrefinalAsync(long tournamentId, long prefinalRoundId);
    Task AddRoundAsync(Round round);
    Task AddRaceAsync(Race race);
    Task RemoveRaceEntriesAsync(IEnumerable<HorseRacing.Domain.Entities.RaceEntry> entries);
    Task<List<HorseRacing.Domain.Entities.RaceEntry>> GetFinalistsFromPreRoundAsync(long tournamentId, long preRoundId);
    Task<bool> HasRaceResultsAsync(IEnumerable<long> raceIds);
    Task AddPrizeAsync(HorseRacing.Domain.Entities.Financials.Prize prize);
    Task ClearRoundsAndRacesAsync(long tournamentId);
    Task<List<HorseRacing.Domain.Entities.MedicalCheckRecord>> GetMedicalCheckRecordsForTournamentAsync(long tournamentId);
    Task<List<HorseRacing.Domain.Entities.Registration>> GetRegistrationsByTournamentIdAsync(long tournamentId);
    Task<bool> HasOverlappingTournamentAsync(DateTime startDate, DateTime endDate, long? excludeTournamentId = null);
    Task<bool> HasRacesMissingRefereesAsync(long tournamentId);
    Task<bool> HasCompleteLaneAssignmentsAsync(long tournamentId);
    Task<Dictionary<long, (bool HasCompleteLaneAssignments, bool HasMissingReferees)>> GetReadinessByTournamentIdsAsync(IEnumerable<long> tournamentIds);
    Task<List<int>> GetAdminUserIdsAsync();

    /// <summary>
    /// Cancel registrations (Pending/PendingVet/Approved) that don't have an accepted/active jockey contract.
    /// Also cancels any pending jockey contracts for those registrations.
    /// Returns info about cancelled registrations for notification purposes.
    /// </summary>
    Task<List<CancelledRegistrationInfo>> CancelRegistrationsWithoutJockeyAsync(long tournamentId);
    Task<List<CancelledRegistrationInfo>> CancelPendingRegistrationsAsync(long tournamentId);
}

