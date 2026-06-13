using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RefereeAssignmentRepository : IRefereeAssignmentRepository
{
    private readonly AppDbContext _context;

    public RefereeAssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Race?> GetRaceByIdAsync(long raceId)
    {
        return await _context.Races
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RaceId == raceId);
    }

    public async Task<RefereeProfile?> GetRefereeProfileByIdAsync(int refereeId)
    {
        return await _context.RefereeProfiles
            .AsNoTracking()
            .Include(rp => rp.User)
                .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(rp => rp.RefereeId == refereeId);
    }

    public async Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, int refereeId)
    {
        return await _context.RaceRefereeAssignments
            .FirstOrDefaultAsync(rra => rra.RaceId == raceId && rra.RefereeId == refereeId);
    }

    public async Task<List<RaceRefereeAssignment>> GetAssignmentsForRaceAsync(long raceId)
    {
        return await _context.RaceRefereeAssignments
            .AsNoTracking()
            .Include(rra => rra.RefereeProfile)
                .ThenInclude(rp => rp!.User)
            .Where(rra => rra.RaceId == raceId)
            .OrderBy(rra => rra.AssignedAt)
            .ToListAsync();
    }

    public async Task AddAssignmentAsync(RaceRefereeAssignment assignment)
    {
        await _context.RaceRefereeAssignments.AddAsync(assignment);
    }

    public void RemoveAssignment(RaceRefereeAssignment assignment)
    {
        _context.RaceRefereeAssignments.Remove(assignment);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
