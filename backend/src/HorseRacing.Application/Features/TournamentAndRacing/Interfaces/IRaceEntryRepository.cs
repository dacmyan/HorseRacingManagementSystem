using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface IRaceEntryRepository
{
    Task<List<RaceEntry>> GetEntriesByRaceIdAsync(long raceId);
}
