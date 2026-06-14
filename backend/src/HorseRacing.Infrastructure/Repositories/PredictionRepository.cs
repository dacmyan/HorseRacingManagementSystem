using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class PredictionRepository : IPredictionRepository
{
    private readonly AppDbContext _context;

    public PredictionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Prediction?> GetByIdAsync(int id)
    {
        return await _context.Predictions
            .Include(p => p.User)
            .Include(p => p.Race)
            .Include(p => p.RaceEntry)
            .FirstOrDefaultAsync(p => p.PredictionId == id);
    }

    public async Task<Prediction?> GetByUserIdAndRaceIdAsync(int userId, long raceId)
    {
        return await _context.Predictions
            .Include(p => p.User)
            .Include(p => p.Race)
            .Include(p => p.RaceEntry)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.RaceId == raceId);
    }

    public async Task<IEnumerable<Prediction>> GetByUserIdAsync(int userId)
    {
        return await _context.Predictions
            .Include(p => p.Race)
            .Include(p => p.RaceEntry)
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prediction>> GetByRaceIdAsync(long raceId)
    {
        return await _context.Predictions
            .Include(p => p.User)
            .Include(p => p.RaceEntry)
            .Where(p => p.RaceId == raceId)
            .ToListAsync();
    }

    public async Task AddAsync(Prediction prediction)
    {
        await _context.Predictions.AddAsync(prediction);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsSpectatorAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        return user != null && user.Role != null && user.Role.Name.Equals("Spectator", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> RaceExistsAsync(long raceId)
    {
        return await _context.Races.AnyAsync(r => r.RaceId == raceId);
    }

    public async Task<bool> RaceEntryExistsAsync(long raceId, long raceEntryId)
    {
        return await _context.RaceEntries.AnyAsync(re => re.RaceId == raceId && re.RaceEntryId == raceEntryId);
    }

    public async Task<string?> GetRaceStatusAsync(long raceId)
    {
        var race = await _context.Races.FirstOrDefaultAsync(r => r.RaceId == raceId);
        return race?.Status;
    }
}
