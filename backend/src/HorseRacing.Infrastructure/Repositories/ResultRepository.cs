using System;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly AppDbContext _context;

    public ResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Race?> GetRaceByIdAsync(long raceId)
    {
        return await _context.Races
            .Include(r => r.Round)
                .ThenInclude(round => round!.Tournament)
            .FirstOrDefaultAsync(r => r.RaceId == raceId);
    }

    public async Task<RaceResult?> GetResultByRaceIdAsync(long raceId)
    {
        return await _context.RaceResults
            .FirstOrDefaultAsync(rr => rr.RaceId == raceId);
    }

    public async Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, int refereeId)
    {
        return await _context.RaceRefereeAssignments
            .FirstOrDefaultAsync(rra => rra.RaceId == raceId && rra.RefereeId == refereeId);
    }

    public async Task<Horse?> GetHorseByIdOrNameAsync(string identifier)
    {
        if (int.TryParse(identifier, out int id))
        {
            return await _context.Horses
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(h => h.HorseId == id);
        }
        return await _context.Horses
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.Name.ToLower() == identifier.ToLower());
    }

    public async Task<RaceEntry?> GetRaceEntryByHorseIdAsync(long raceId, long horseId)
    {
        return await _context.RaceEntries
            .Include(re => re.Registration)
                .ThenInclude(reg => reg!.Horse)
            .Include(re => re.JockeyProfile)
                .ThenInclude(jp => jp!.User)
            .FirstOrDefaultAsync(re => re.RaceId == raceId && re.Registration != null && re.Registration.HorseId == horseId);
    }

    public async Task AddResultAsync(RaceResult result)
    {
        await _context.RaceResults.AddAsync(result);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
