using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RaceRepository : IRaceRepository
{
    private readonly AppDbContext _context;

    public RaceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Race race)
    {
        await _context.Races.AddAsync(race);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Race>> GetPublicRaceScheduleAsync()
    {
        return await _context.Races
            .AsNoTracking()
            .Include(r => r.Round)
                .ThenInclude(round => round!.Tournament)
            .OrderBy(r => r.RaceDate)
            .ToListAsync();
    }

    public async Task<Round?> GetRoundByIdAsync(long roundId)
    {
        return await _context.Rounds
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoundId == roundId);
    }

    public async Task<Race?> GetByIdWithDetailsAsync(long raceId)
    {
        return await _context.Races
            .AsNoTracking()
            .Include(r => r.Round)
                .ThenInclude(round => round!.Tournament)
            .FirstOrDefaultAsync(r => r.RaceId == raceId);
    }

    public async Task<Registration?> GetRegistrationWithHorseAsync(long registrationId)
    {
        return await _context.Registrations
            .AsNoTracking()
            .Include(r => r.Horse)
            .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);
    }

    public async Task<List<RaceEntry>> GetRaceEntriesAsync(long raceId)
    {
        return await _context.RaceEntries
            .AsNoTracking()
            .Include(re => re.Registration)
                .ThenInclude(r => r!.Horse)
            .Include(re => re.JockeyProfile)
                .ThenInclude(j => j!.User)
            .Where(re => re.RaceId == raceId)
            .ToListAsync();
    }

    public async Task<bool> HasActiveJockeyContractAsync(long tournamentId, long horseId, long jockeyId)
    {
        if (horseId < int.MinValue || horseId > int.MaxValue)
        {
            return false;
        }
        if (jockeyId < int.MinValue || jockeyId > int.MaxValue)
        {
            return false;
        }

        var intHorseId = (int)horseId;
        var intJockeyId = (int)jockeyId;

        var jockeyProfile = await _context.JockeyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(jp => jp.JockeyId == intJockeyId);

        if (jockeyProfile == null)
        {
            return false;
        }

        return await _context.JockeyContracts
            .AsNoTracking()
            .AnyAsync(jc => jc.HorseId == intHorseId 
                && jc.JockeyId == jockeyProfile.UserId 
                && jc.Status == "Active");
    }

    public async Task AddRaceEntryAsync(RaceEntry raceEntry)
    {
        await _context.RaceEntries.AddAsync(raceEntry);
    }
}
