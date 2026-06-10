using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;
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

    public async Task<IEnumerable<Bet>> GetByRaceIdAsync(int raceId)
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

    public async Task<Race?> GetRaceByIdAsync(int raceId)
    {
        return await _context.Races.FirstOrDefaultAsync(r => r.Id == raceId);
    }

    public async Task<bool> IsHorseInRaceAsync(int raceId, int horseId)
    {
        return await _context.RaceEntries
            .AnyAsync(re => re.RaceId == raceId && re.HorseId == horseId);
    }

    public async Task<RaceResult?> GetRaceResultAsync(int raceId)
    {
        return await _context.RaceResults.FirstOrDefaultAsync(rr => rr.RaceId == raceId);
    }

    public async Task<RaceEntry?> GetRaceEntryAsync(int raceId, int horseId)
    {
        return await _context.RaceEntries.FirstOrDefaultAsync(re => re.RaceId == raceId && re.HorseId == horseId);
    }

    public async Task<Horse?> GetHorseByIdOrNameAsync(string identifier)
    {
        if (int.TryParse(identifier, out int id))
        {
            return await _context.Horses.Include(h => h.Owner).FirstOrDefaultAsync(h => h.Id == id);
        }
        return await _context.Horses.Include(h => h.Owner).FirstOrDefaultAsync(h => h.Name.ToLower() == identifier.ToLower());
    }

    public async Task<Tournament?> GetTournamentByIdAsync(int tournamentId)
    {
        return await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
    }

    public async Task<Race?> GetFinalRaceInTournamentAsync(int tournamentId)
    {
        return await _context.Races
            .Where(r => r.TournamentId == tournamentId && r.Status == "Finished")
            .OrderByDescending(r => r.ScheduledTime)
            .FirstOrDefaultAsync();
    }

    public async Task<Prediction?> GetPredictionAsync(int raceId, int userId)
    {
        return await _context.Predictions
            .FirstOrDefaultAsync(p => p.RaceId == raceId && p.UserId == userId);
    }

    public async Task<IEnumerable<Prediction>> GetPredictionsByRaceIdAsync(int raceId)
    {
        return await _context.Predictions
            .Where(p => p.RaceId == raceId)
            .ToListAsync();
    }

    public async Task AddPredictionAsync(Prediction prediction)
    {
        await _context.Predictions.AddAsync(prediction);
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
