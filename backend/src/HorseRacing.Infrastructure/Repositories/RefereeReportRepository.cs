using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RefereeReportRepository : IRefereeReportRepository
{
    private readonly AppDbContext _context;

    public RefereeReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RaceRefereeAssignment?> GetAssignmentByIdAsync(long assignmentId)
    {
        return await _context.RaceRefereeAssignments
            .Include(a => a.Race)
            .Include(a => a.RefereeProfile)
                .ThenInclude(rp => rp!.User)
                    .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    }

    public async Task<RaceRefereeAssignment?> GetAssignmentByRaceAndRefereeAsync(long raceId, int refereeId)
    {
        return await _context.RaceRefereeAssignments
            .Include(a => a.Race)
            .Include(a => a.RefereeProfile)
                .ThenInclude(rp => rp!.User)
                    .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(a => a.RaceId == raceId && a.RefereeId == refereeId);
    }

    public async Task<bool> RaceExistsAsync(long raceId)
    {
        return await _context.Races
            .AsNoTracking()
            .AnyAsync(r => r.RaceId == raceId);
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == userId);
    }

    public async Task<bool> HorseExistsAsync(long horseId)
    {
        return await _context.Horses
            .AsNoTracking()
            .AnyAsync(h => h.HorseId == horseId);
    }

    public async Task AddReportAsync(RefereeReport report)
    {
        await _context.RefereeReports.AddAsync(report);
    }

    public async Task<List<RefereeReport>> GetReportsByRaceIdAsync(long raceId)
    {
        return await _context.RefereeReports
            .AsNoTracking()
            .Include(r => r.Assignment)
                .ThenInclude(a => a!.Race)
            .Include(r => r.Assignment)
                .ThenInclude(a => a!.RefereeProfile)
                    .ThenInclude(rp => rp!.User)
            .Where(r => r.Assignment!.RaceId == raceId)
            .OrderBy(r => r.ReportId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
