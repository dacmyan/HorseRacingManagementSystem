using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public interface ITournamentService
{
    Task<TournamentResponse> CreateTournamentAsync(CreateTournamentRequest request);
}
