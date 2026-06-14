using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRaceResultService
{
    Task<RaceResultResponse> SubmitResultAsync(SubmitRaceResultRequest request);
    Task<RaceResultResponse> PublishResultAsync(long raceId);
    Task<List<RaceResultResponse>?> GetResultsByRaceIdAsync(long raceId);
    Task<List<RaceResultResponse>?> GetPublicResultsByRaceIdAsync(long raceId);
}
