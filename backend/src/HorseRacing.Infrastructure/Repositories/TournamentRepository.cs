using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly AppDbContext _context;

    public TournamentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Tournament tournament)
    {
        await _context.Tournaments.AddAsync(tournament);
    }

    public void Update(Tournament tournament)
    {
        _context.Tournaments.Update(tournament);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(long tournamentId)
    {
        return await _context.Tournaments
            .AsNoTracking()
            .AnyAsync(t => t.TournamentId == tournamentId);
    }

    public async Task<Tournament?> GetByIdAsync(long tournamentId)
    {
        return await _context.Tournaments
            .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
    }

    public async Task<Tournament?> GetByIdWithRoundsAsync(long tournamentId)
    {
        return await _context.Tournaments
            .Include(t => t.Rounds)
            .FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
    }

    public async Task<List<Tournament>> GetAllAsync()
    {
        return await _context.Tournaments
            .AsNoTracking()
            .Include(t => t.Rounds)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }

    public async Task<List<HorseRacing.Domain.Entities.Registration>> GetApprovedRegistrationsAsync(long tournamentId)
    {
        return await _context.Set<HorseRacing.Domain.Entities.Registration>()
            .Include(r => r.Horse)
            .Where(r => r.TournamentId == tournamentId && r.Status == "Approved")
            .OrderBy(r => r.RegistrationId)
            .ToListAsync();
    }

    public async Task AddRacesAsync(IEnumerable<HorseRacing.Domain.Entities.Tournaments.Race> races)
    {
        await _context.Races.AddRangeAsync(races);
    }

    public async Task AddRaceEntriesAsync(IEnumerable<HorseRacing.Domain.Entities.RaceEntry> entries)
    {
        await _context.RaceEntries.AddRangeAsync(entries);
    }

    public async Task<List<Race>> GetRacesByRoundIdAsync(long roundId)
    {
        return await _context.Races
            .Where(r => r.RoundId == roundId)
            .ToListAsync();
    }

    public async Task<List<HorseRacing.Domain.Entities.RaceEntry>> GetRaceEntriesByRaceIdAsync(long raceId)
    {
        return await _context.RaceEntries
            .Where(re => re.RaceId == raceId)
            .ToListAsync();
    }

    public async Task<Dictionary<long, int>> GetActiveJockeyProfileIdsByHorseAsync(long tournamentId, IEnumerable<long> horseIds)
    {
        var intHorseIds = horseIds
            .Where(id => id >= int.MinValue && id <= int.MaxValue)
            .Select(id => (int)id)
            .Distinct()
            .ToList();

        if (intHorseIds.Count == 0)
        {
            return new Dictionary<long, int>();
        }

        var assignments = await (
            from contract in _context.JockeyContracts.AsNoTracking()
            join profile in _context.JockeyProfiles.AsNoTracking()
                on contract.JockeyId equals profile.UserId
            where contract.TournamentId == tournamentId
                && intHorseIds.Contains((int)contract.HorseId)
                && contract.Status == "Active"
                && profile.Status == "Active"
            select new
            {
                contract.HorseId,
                profile.JockeyId
            })
            .ToListAsync();

        return assignments
            .GroupBy(x => x.HorseId)
            .ToDictionary(g => g.Key, g => g.First().JockeyId);
    }

    public async Task<List<HorseRacing.Domain.Entities.Registration>> GetTopHorsesFromPrefinalAsync(long tournamentId, long prefinalRoundId)
    {
        var races = await _context.Races
            .Where(r => r.RoundId == prefinalRoundId)
            .ToListAsync();

        if (!races.Any())
        {
            return new List<HorseRacing.Domain.Entities.Registration>();
        }

        if (races.Any(r => r.Status != "Finished"))
        {
            throw new InvalidOperationException("Not all pre-final races are finished yet.");
        }

        var raceIds = races.Select(r => r.RaceId).ToList();
        var results = await _context.RaceResults
            .Where(rr => raceIds.Contains(rr.RaceId))
            .ToListAsync();

        var winnersList = new List<long>();
        foreach (var result in results)
        {
            if (string.IsNullOrEmpty(result.Winner)) continue;
            if (int.TryParse(result.Winner, out int id))
            {
                winnersList.Add(id);
            }
            else
            {
                var horse = await _context.Horses
                    .FirstOrDefaultAsync(h => h.Name.ToLower() == result.Winner.ToLower());
                if (horse != null)
                {
                    winnersList.Add(horse.HorseId);
                }
            }
        }

        winnersList = winnersList.Distinct().ToList();

        var allPrefinalEntries = await _context.RaceEntries
            .Include(re => re.Registration)
            .Where(re => raceIds.Contains(re.RaceId))
            .ToListAsync();

        var selectedHorseIds = new List<long>();

        // Winners first (if not disqualified/scratched)
        foreach (var winHorseId in winnersList)
        {
            var entry = allPrefinalEntries.FirstOrDefault(e => e.Registration?.HorseId == winHorseId);
            if (entry != null && (entry.Status == "Disqualified" || entry.Status == "Scratched"))
            {
                continue;
            }
            selectedHorseIds.Add(winHorseId);
        }

        // Fill remaining spots up to 12
        foreach (var entry in allPrefinalEntries)
        {
            if (entry.Registration == null) continue;
            if (selectedHorseIds.Count >= 12) break;

            if (entry.Status == "Disqualified" || entry.Status == "Scratched" || entry.Status != "Confirmed")
            {
                continue;
            }

            if (!selectedHorseIds.Contains(entry.Registration.HorseId))
            {
                selectedHorseIds.Add(entry.Registration.HorseId);
            }
        }

        return await _context.Registrations
            .Include(r => r.Horse)
            .Where(r => r.TournamentId == tournamentId && selectedHorseIds.Contains(r.HorseId))
            .ToListAsync();
    }

    public async Task AddRoundAsync(Round round)
    {
        await _context.Rounds.AddAsync(round);
    }

    public async Task AddRaceAsync(Race race)
    {
        await _context.Races.AddAsync(race);
    }

    public async Task RemoveRaceEntriesAsync(IEnumerable<HorseRacing.Domain.Entities.RaceEntry> entries)
    {
        _context.RaceEntries.RemoveRange(entries);
        await Task.CompletedTask;
    }

    public async Task<List<HorseRacing.Domain.Entities.RaceEntry>> GetFinalistsFromPreRoundAsync(long tournamentId, long preRoundId)
    {
        return await _context.RaceEntries
            .Include(re => re.Registration)
                .ThenInclude(reg => reg!.Horse)
            .Include(re => re.Race)
            .Where(re => re.Race.RoundId == preRoundId)
            .Where(re => re.FinishTime.HasValue && re.FinishTime.Value > 0)
            .Where(re => re.Race.Status == "Completed" || re.Race.Status == "Finished")
            .ToListAsync();
    }

    public async Task<bool> HasRaceResultsAsync(IEnumerable<long> raceIds)
    {
        return await _context.RaceResults.AnyAsync(rr => raceIds.Contains(rr.RaceId));
    }
}
