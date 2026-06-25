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
}
