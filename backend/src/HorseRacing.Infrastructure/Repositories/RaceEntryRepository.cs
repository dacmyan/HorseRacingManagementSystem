using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RaceEntryRepository : IRaceEntryRepository
{
    private readonly AppDbContext _context;

    public RaceEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RaceEntry>> GetEntriesByRaceIdAsync(long raceId)
    {
        return await _context.RaceEntries
            .AsNoTracking()
            .Include(re => re.Horse)
            .Include(re => re.Jockey)
            .Where(re => re.RaceId == raceId)
            .OrderBy(re => re.Id)
            .ToListAsync();
    }
}
