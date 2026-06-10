using System.Threading.Tasks;
using HorseRacing.Application.Features.FinancialRewards.DTOs;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IPrizePayoutService
{
    Task ProcessPrizePayoutAsync(PrizePayoutRequest request);
}
