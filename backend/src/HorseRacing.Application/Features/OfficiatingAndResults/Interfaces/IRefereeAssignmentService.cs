using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRefereeAssignmentService
{
    Task<RaceRefereeResponse> AssignRefereeAsync(long raceId, AssignRefereeRequest request);
    Task<List<RaceRefereeResponse>> GetAssignedRefereesAsync(long raceId);
    Task RemoveRefereeAssignmentAsync(long raceId, int refereeId);
}
