using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RoundRepository : IRoundRepository
{
    private readonly AppDbContext _context;

    public RoundRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Round>> GetRoundsByTournamentIdAsync(long tournamentId)
    {
        return await _context.Rounds
            .AsNoTracking()
            .Include(r => r.Tournament)
            .Include(r => r.Races.OrderBy(race => race.RaceDate))
            .Where(r => r.TournamentId == tournamentId)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();
    }

    public async Task<Round?> GetRoundWithDetailsAsync(long roundId)
    {
        return await _context.Rounds
            .AsNoTracking()
            .Include(r => r.Tournament)
            .Include(r => r.Races.OrderBy(race => race.RaceDate))
            .FirstOrDefaultAsync(r => r.RoundId == roundId);
    }
}
