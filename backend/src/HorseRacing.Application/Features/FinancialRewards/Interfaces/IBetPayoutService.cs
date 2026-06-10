using System.Threading.Tasks;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IBetPayoutService
{
    Task ProcessPayoutAsync(int raceId);
}
