using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRefereeReportRepository
{
    Task<RaceRefereeAssignment?> GetAssignmentByIdAsync(long assignmentId);
    Task<RaceRefereeAssignment?> GetAssignmentByRaceAndRefereeAsync(long raceId, int refereeId);
    Task<bool> RaceExistsAsync(long raceId);
    Task<bool> UserExistsAsync(int userId);
    Task<bool> HorseExistsAsync(long horseId);
    Task AddReportAsync(RefereeReport report);
    Task<List<RefereeReport>> GetReportsByRaceIdAsync(long raceId);
    Task SaveChangesAsync();
}
