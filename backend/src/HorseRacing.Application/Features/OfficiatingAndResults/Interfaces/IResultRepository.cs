using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;

public interface IResultRepository
{
    Task<Race?> GetRaceByIdAsync(long raceId);
    Task<RaceResult?> GetResultByRaceIdAsync(long raceId);
    Task<RaceRefereeAssignment?> GetAssignmentAsync(long raceId, int refereeId);
    Task<Horse?> GetHorseByIdOrNameAsync(string identifier);
    Task<RaceEntry?> GetRaceEntryByHorseIdAsync(long raceId, long horseId);
    Task AddResultAsync(RaceResult result);
    Task SaveChangesAsync();
}
