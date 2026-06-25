using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface ITournamentRepository
{
    Task AddAsync(Tournament tournament);
    void Update(Tournament tournament);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(long tournamentId);
    Task<Tournament?> GetByIdAsync(long tournamentId);
    Task<Tournament?> GetByIdWithRoundsAsync(long tournamentId);
    Task<List<Tournament>> GetAllAsync();
    Task<List<HorseRacing.Domain.Entities.Registration>> GetApprovedRegistrationsAsync(long tournamentId);
    Task AddRacesAsync(IEnumerable<HorseRacing.Domain.Entities.Tournaments.Race> races);
    Task AddRaceEntriesAsync(IEnumerable<HorseRacing.Domain.Entities.RaceEntry> entries);
}
