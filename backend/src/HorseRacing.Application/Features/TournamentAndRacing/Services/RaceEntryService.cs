using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class RaceEntryService : IRaceEntryService
{
    private readonly IRaceRepository _raceRepository;
    private readonly IRaceEntryRepository _raceEntryRepository;

    public RaceEntryService(IRaceRepository raceRepository, IRaceEntryRepository raceEntryRepository)
    {
        _raceRepository = raceRepository;
        _raceEntryRepository = raceEntryRepository;
    }

    public async Task<List<RaceEntryResponse>?> GetEntriesByRaceIdAsync(long raceId)
    {
        // Check if race exists by reusing existing GetByIdWithDetailsAsync from IRaceRepository
        var race = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (race == null)
        {
            return null;
        }

        var entries = await _raceEntryRepository.GetEntriesByRaceIdAsync(raceId);

        return entries.Select(e => new RaceEntryResponse
        {
            EntryId = e.Id,
            RaceId = e.RaceId,
            HorseId = e.HorseId,
            HorseName = e.Horse?.Name ?? string.Empty,
            JockeyId = e.JockeyId,
            JockeyName = e.Jockey?.FullName ?? string.Empty,
            Status = e.Status
        }).ToList();
    }
}
