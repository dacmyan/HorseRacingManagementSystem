using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class BetRepository : IBetRepository
{
    private readonly AppDbContext _context;

    public BetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Bet?> GetByIdAsync(int id)
    {
        return await _context.Bets
            .Include(b => b.User)
            .Include(b => b.Race)
            .Include(b => b.Horse)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bet>> GetByUserIdAsync(int userId)
    {
        return await _context.Bets
            .Include(b => b.Race)
            .Include(b => b.Horse)
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bet>> GetByRaceIdAsync(long raceId)
    {
        return await _context.Bets
            .Include(b => b.User)
            .Include(b => b.Horse)
            .Where(b => b.RaceId == raceId)
            .ToListAsync();
    }

    public async Task AddAsync(Bet bet)
    {
        await _context.Bets.AddAsync(bet);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Race?> GetRaceByIdAsync(long raceId)
    {
        return await _context.Races
            .Include(r => r.Round)
            .ThenInclude(round => round.Tournament)
            .FirstOrDefaultAsync(r => r.RaceId == raceId);
    }

    public async Task<bool> IsHorseInRaceAsync(long raceId, int horseId)
    {
        return await _context.RaceEntries
            .AnyAsync(re => re.RaceId == raceId && re.Registration != null && re.Registration.HorseId == horseId);
    }

    public async Task<RaceResult?> GetRaceResultAsync(long raceId)
    {
        return await _context.RaceResults.FirstOrDefaultAsync(rr => rr.RaceId == raceId);
    }

    public async Task<RaceEntry?> GetRaceEntryAsync(long raceId, int horseId)
    {
        return await _context.RaceEntries
            .Include(re => re.Registration)
            .Include(re => re.JockeyProfile)
            .FirstOrDefaultAsync(re => re.RaceId == raceId && re.Registration != null && re.Registration.HorseId == horseId);
    }

    public async Task<Horse?> GetHorseByIdOrNameAsync(string identifier)
    {
        if (int.TryParse(identifier, out int id))
        {
            return await _context.Horses.Include(h => h.Owner).FirstOrDefaultAsync(h => h.HorseId == id);
        }
        return await _context.Horses.Include(h => h.Owner).FirstOrDefaultAsync(h => h.Name.ToLower() == identifier.ToLower());
    }

    public async Task<Tournament?> GetTournamentByIdAsync(long tournamentId)
    {
        return await _context.Tournaments.FirstOrDefaultAsync(t => t.TournamentId == tournamentId);
    }

    public async Task<Race?> GetFinalRaceInTournamentAsync(long tournamentId)
    {
        return await _context.Races
            .Where(r => r.Round != null && r.Round.TournamentId == tournamentId && r.Status == "Finished")
            .OrderByDescending(r => r.RaceDate)
            .FirstOrDefaultAsync();
    }



    public async Task<IEnumerable<JockeyProfile>> GetJockeyRankingsAsync()
    {
        return await _context.JockeyProfiles
            .Include(jp => jp.User)
            .Where(jp => jp.Status == "Active")
            .OrderByDescending(jp => jp.RankingPoint)
            .ToListAsync();
    }

    public async Task<IEnumerable<Horse>> GetHorsesWithWinnersAsync()
    {
        return await _context.Horses
            .Include(h => h.Owner)
            .ToListAsync();
    }

    public async Task<IEnumerable<RaceResult>> GetFinishedRaceResultsAsync()
    {
        return await _context.RaceResults.ToListAsync();
    }
}
