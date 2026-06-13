using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly AppDbContext _context;

    public RegistrationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Registration?> GetByIdAsync(long id)
    {
        return await _context.Registrations
            .Include(r => r.Horse)
            .Include(r => r.Tournament)
            .FirstOrDefaultAsync(r => r.RegistrationId == id);
    }

    public async Task<Registration?> GetByHorseIdAndTournamentIdAsync(long horseId, long tournamentId)
    {
        return await _context.Registrations
            .FirstOrDefaultAsync(r => r.HorseId == horseId && r.TournamentId == tournamentId);
    }

    public async Task<IEnumerable<Registration>> GetByOwnerIdAsync(int ownerUserId)
    {
        return await _context.Registrations
            .Include(r => r.Horse)
            .Include(r => r.Tournament)
            .Where(r => r.Horse != null && r.Horse.OwnerId == ownerUserId)
            .ToListAsync();
    }

    public async Task AddAsync(Registration registration)
    {
        await _context.Registrations.AddAsync(registration);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
