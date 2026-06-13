using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRefereeService
{
    Task<ViolationResponse> LogViolationAsync(LogViolationRequest request);
    Task<List<ViolationResponse>?> GetViolationsByRaceIdAsync(long raceId);
    Task<RefereeReportResponse> SubmitReportAsync(CreateRefereeReportRequest request);
    Task<List<RefereeReportResponse>?> GetReportsByRaceIdAsync(long raceId);
}
