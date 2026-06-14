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
}
