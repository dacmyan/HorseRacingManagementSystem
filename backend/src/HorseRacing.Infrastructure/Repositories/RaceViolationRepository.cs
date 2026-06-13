using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RaceViolationRepository : IRaceViolationRepository
{
    private readonly AppDbContext _context;

    public RaceViolationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Race?> GetRaceByIdAsync(long raceId)
    {
        return await _context.Races
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RaceId == raceId);
    }

    public async Task<RaceEntry?> GetRaceEntryByIdAsync(long raceEntryId)
    {
        return await _context.RaceEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(re => re.RaceEntryId == raceEntryId);
    }

    public async Task<RefereeProfile?> GetRefereeProfileByIdAsync(long refereeId)
    {
        return await _context.RefereeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(rp => rp.RefereeId == refereeId);
    }

    public async Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, long refereeId)
    {
        return await _context.RaceRefereeAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.RaceId == raceId && a.RefereeId == refereeId);
    }

    public async Task<List<RaceViolation>> GetViolationsByRaceIdAsync(long raceId)
    {
        return await _context.Violations
            .AsNoTracking()
            .Include(rv => rv.RaceEntry)
            .Include(rv => rv.RefereeProfile)
            .Where(rv => rv.RaceId == raceId)
            .ToListAsync();
    }

    public async Task AddViolationAsync(RaceViolation violation)
    {
        await _context.Violations.AddAsync(violation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
