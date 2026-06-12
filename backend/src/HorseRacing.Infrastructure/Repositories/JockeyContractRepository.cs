using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class JockeyContractRepository : IJockeyContractRepository
{
    private readonly AppDbContext _context;

    public JockeyContractRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JockeyContract?> GetByIdAsync(int id)
    {
        return await _context.JockeyContracts
            .Include(jc => jc.Horse)
            .Include(jc => jc.Owner)
            .Include(jc => jc.Jockey)
            .FirstOrDefaultAsync(jc => jc.Id == id);
    }

    public async Task<IEnumerable<JockeyContract>> GetByJockeyIdAsync(int jockeyUserId)
    {
        return await _context.JockeyContracts
            .Include(jc => jc.Horse)
            .Include(jc => jc.Owner)
            .Where(jc => jc.JockeyId == jockeyUserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<JockeyContract>> GetByOwnerIdAsync(int ownerUserId)
    {
        return await _context.JockeyContracts
            .Include(jc => jc.Horse)
            .Include(jc => jc.Jockey)
            .Where(jc => jc.OwnerId == ownerUserId)
            .ToListAsync();
    }

    public async Task<JockeyContract?> GetActiveContractForHorseAsync(int horseId)
    {
        return await _context.JockeyContracts
            .Include(jc => jc.Jockey)
            .FirstOrDefaultAsync(jc => jc.HorseId == horseId && jc.Status == "Active");
    }

    public async Task AddAsync(JockeyContract contract)
    {
        await _context.JockeyContracts.AddAsync(contract);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
