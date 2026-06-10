using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;

namespace HorseRacing.Infrastructure.Repositories;

public class PayoutRepository : IPayoutRepository
{
    private readonly AppDbContext _context;

    public PayoutRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payout payout)
    {
        await _context.Payouts.AddAsync(payout);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
