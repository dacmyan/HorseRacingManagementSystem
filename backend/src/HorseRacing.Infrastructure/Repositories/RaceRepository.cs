using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
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
        .Include(r => r.Round)
            .ThenInclude(round => round!.Tournament)
        .ToListAsync();
}
}
