using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Financials;

namespace HorseRacing.Application.Features.FinancialRewards.Interfaces;

/// <summary>Wraps a database transaction, abstracting EF Core from the Application layer.</summary>
public interface IDbTransaction : System.IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}

public interface IPrizeRepository
{
    Task<Prize?> GetByTournamentAndRankAsync(int tournamentId, int rank);
    Task AddAsync(Prize prize);
    Task AddTournamentPrizePayoutAsync(TournamentPrizePayout payout);
    Task SaveChangesAsync();
    Task<IDbTransaction> BeginTransactionAsync();
}
