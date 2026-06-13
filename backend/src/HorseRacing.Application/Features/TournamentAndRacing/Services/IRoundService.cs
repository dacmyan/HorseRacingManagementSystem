using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public interface IRoundService
{
    Task<List<RoundDetailResponse>?> GetRoundsByTournamentIdAsync(long tournamentId);
    Task<RoundDetailResponse?> GetRoundByIdAsync(long roundId);
}
