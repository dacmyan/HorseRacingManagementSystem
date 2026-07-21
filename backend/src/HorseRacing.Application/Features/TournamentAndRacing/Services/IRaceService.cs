using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public interface IRaceService
{
    Task<RaceScheduleResponse> CreateRaceAsync(CreateRaceRequest request);
    Task<List<RaceScheduleResponse>> GetPublicRaceScheduleAsync();
    Task<RaceEntryResponse> CreateRaceEntryAsync(long raceId, CreateRaceEntryRequest request);
    Task<RaceScheduleResponse?> GetRaceByIdAsync(long raceId);
    Task<List<RaceEntryResponse>?> GetRaceEntriesByRaceIdAsync(long raceId);
    Task DeleteRaceAsync(long raceId);
    Task<RaceScheduleResponse?> UpdateRaceAsync(long raceId, UpdateRaceRequest request);
}
