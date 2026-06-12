using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Financials;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

public interface IPrizeRepository
{
    Task<Prize?> GetByTournamentAndRankAsync(int tournamentId, int rank);
    Task AddAsync(Prize prize);
    Task AddTournamentPrizePayoutAsync(TournamentPrizePayout payout);
    Task SaveChangesAsync();
}
