using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IRaceViolationRepository
{
    Task<Race?> GetRaceByIdAsync(long raceId);
    Task<RaceEntry?> GetRaceEntryByIdAsync(long raceEntryId);
    Task<RefereeProfile?> GetRefereeProfileByIdAsync(long refereeId);
    Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, long refereeId);
    Task<List<RaceViolation>> GetViolationsByRaceIdAsync(long raceId);
    Task AddViolationAsync(RaceViolation violation);
    Task SaveChangesAsync();
}
