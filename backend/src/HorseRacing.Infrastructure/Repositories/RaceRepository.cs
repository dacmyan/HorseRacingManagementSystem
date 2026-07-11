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

    public async Task<bool> HasActiveJockeyContractAsync(long tournamentId, long horseId, int jockeyId)
    {
        if (horseId < int.MinValue || horseId > int.MaxValue)
        {
            return false;
        }

        var intHorseId = (int)horseId;

        var jockeyProfile = await _context.JockeyProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(jp => jp.JockeyId == jockeyId);

        if (jockeyProfile == null)
        {
            return false;
        }

        return await _context.JockeyContracts
            .AsNoTracking()
            .AnyAsync(jc => jc.TournamentId == tournamentId
                && jc.HorseId == intHorseId 
                && jc.JockeyId == jockeyProfile.UserId 
                && (jc.Status == "Active" || jc.Status == "Accepted"));
    }

    public async Task<(int JockeyProfileId, string JockeyName)?> GetActiveJockeyForHorseAsync(long tournamentId, long horseId)
    {
        if (horseId < int.MinValue || horseId > int.MaxValue)
        {
            return null;
        }

        var intHorseId = (int)horseId;
        var assignment = await (
            from contract in _context.JockeyContracts.AsNoTracking()
            join profile in _context.JockeyProfiles.AsNoTracking()
                on contract.JockeyId equals profile.UserId
            join user in _context.Users.AsNoTracking()
                on profile.UserId equals user.UserId
            where contract.TournamentId == tournamentId
                && contract.HorseId == intHorseId
                && (contract.Status == "Active" || contract.Status == "Accepted")
                && profile.Status == "Active"
            select new
            {
                profile.JockeyId,
                user.FullName
            })
            .FirstOrDefaultAsync();

        return assignment == null
            ? null
            : (assignment.JockeyId, assignment.FullName);
    }

    public async Task AddRaceEntryAsync(RaceEntry raceEntry)
    {
        await _context.RaceEntries.AddAsync(raceEntry);
    }

    public async Task DeleteRaceAsync(long raceId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var raceEntryIds = await _context.RaceEntries
            .Where(re => re.RaceId == raceId)
            .Select(re => re.RaceEntryId)
            .ToListAsync();

        var betIds = await _context.Bets
            .Where(b => b.RaceId == raceId)
            .Select(b => b.Id)
            .ToListAsync();

        var payoutIds = await _context.Payouts
            .Where(p => betIds.Contains(p.BetId))
            .Select(p => p.Id)
            .ToListAsync();

        var assignmentIds = await _context.RaceRefereeAssignments
            .Where(a => a.RaceId == raceId)
            .Select(a => a.AssignmentId)
            .ToListAsync();

        await _context.Transactions
            .Where(t => (t.BetId.HasValue && betIds.Contains(t.BetId.Value))
                || (t.PayoutId.HasValue && payoutIds.Contains(t.PayoutId.Value)))
            .ExecuteDeleteAsync();

        await _context.Payouts
            .Where(p => payoutIds.Contains(p.Id))
            .ExecuteDeleteAsync();

        await _context.Predictions
            .Where(p => p.RaceId == raceId || raceEntryIds.Contains(p.RaceEntryId))
            .ExecuteDeleteAsync();

        await _context.RefereeReports
            .Where(r => assignmentIds.Contains(r.AssignmentId))
            .ExecuteDeleteAsync();

        await _context.Violations
            .Where(v => v.RaceId == raceId)
            .ExecuteDeleteAsync();

        await _context.RaceRefereeAssignments
            .Where(a => assignmentIds.Contains(a.AssignmentId))
            .ExecuteDeleteAsync();

        await _context.RaceResults
            .Where(r => r.RaceId == raceId)
            .ExecuteDeleteAsync();

        await _context.RaceEntries
            .Where(re => re.RaceId == raceId)
            .ExecuteDeleteAsync();

        await _context.Bets
            .Where(b => b.RaceId == raceId)
            .ExecuteDeleteAsync();

        await _context.Races
            .Where(r => r.RaceId == raceId)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task<HashSet<long>> GetRaceIdsWithHealthIssuesAsync(IEnumerable<long> raceIds)
    {
        var list = raceIds.ToList();
        var ids = await _context.RaceEntries
            .AsNoTracking()
            .Include(re => re.Registration)
                .ThenInclude(reg => reg!.Horse)
            .Where(re => list.Contains(re.RaceId))
            .Where(re => re.Registration != null && re.Registration.Horse != null &&
                        (re.Registration.Horse.HealthStatus == "Sick" || re.Registration.Horse.HealthStatus == "Injured"))
            .Select(re => re.RaceId)
            .Distinct()
            .ToListAsync();
        return new HashSet<long>(ids);
    }
}
