using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRaceViolationService
{
    Task<RaceViolationResponse> CreateViolationAsync(long raceId, CreateRaceViolationRequest request);
    Task<List<RaceViolationResponse>> GetViolationsByRaceIdAsync(long raceId);
}
