using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRefereeAssignmentRepository
{
    Task<Race?> GetRaceByIdAsync(long raceId);
    Task<RefereeProfile?> GetRefereeProfileByIdAsync(int refereeId);
    Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, int refereeId);
    Task<List<RaceRefereeAssignment>> GetAssignmentsForRaceAsync(long raceId);
    Task AddAssignmentAsync(RaceRefereeAssignment assignment);
    void RemoveAssignment(RaceRefereeAssignment assignment);
    Task SaveChangesAsync();
}
