using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public interface ITournamentService
{
    Task<TournamentResponse> CreateTournamentAsync(CreateTournamentRequest request);
    Task<TournamentResponse?> UpdateTournamentAsync(long id, UpdateTournamentRequest request);
    Task<List<TournamentResponse>> GetAllTournamentsAsync();
    Task<TournamentResponse?> GetTournamentByIdAsync(long id);
    Task<List<RaceScheduleResponse>> GenerateRacesForTournamentAsync(long tournamentId);
}
