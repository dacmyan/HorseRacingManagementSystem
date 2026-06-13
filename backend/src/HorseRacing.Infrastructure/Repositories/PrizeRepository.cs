using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Financials;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class PrizeRepository : IPrizeRepository
{
    private readonly AppDbContext _context;

    public PrizeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Prize?> GetByTournamentAndRankAsync(int tournamentId, int rank)
    {
        return await _context.Prizes
            .FirstOrDefaultAsync(p => p.TournamentId == tournamentId && p.RankPosition == rank);
    }

    public async Task AddAsync(Prize prize)
    {
        await _context.Prizes.AddAsync(prize);
    }

    public async Task AddTournamentPrizePayoutAsync(TournamentPrizePayout payout)
    {
        await _context.TournamentPrizePayouts.AddAsync(payout);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
