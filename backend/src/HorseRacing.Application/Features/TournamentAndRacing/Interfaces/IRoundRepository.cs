using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface IRoundRepository
{
    Task<List<Round>> GetRoundsByTournamentIdAsync(long tournamentId);
    Task<Round?> GetRoundWithDetailsAsync(long roundId);
}
