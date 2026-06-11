using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface IRaceRepository
{
    Task AddAsync(Race race);
    Task SaveChangesAsync();
    Task<List<Race>> GetPublicRaceScheduleAsync();
    Task<Round?> GetRoundByIdAsync(long roundId);
    Task<Race?> GetByIdWithDetailsAsync(long raceId);
}
