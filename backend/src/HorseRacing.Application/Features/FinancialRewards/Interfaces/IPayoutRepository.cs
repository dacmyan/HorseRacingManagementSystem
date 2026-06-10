using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IPayoutRepository
{
    Task AddAsync(Payout payout);
    Task SaveChangesAsync();
}
