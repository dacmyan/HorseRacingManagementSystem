using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public interface IRaceService
{
    Task<RaceScheduleResponse> CreateRaceAsync(CreateRaceRequest request);
    Task<List<RaceScheduleResponse>> GetPublicRaceScheduleAsync();
}
