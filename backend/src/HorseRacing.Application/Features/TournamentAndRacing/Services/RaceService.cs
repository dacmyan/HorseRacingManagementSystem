using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class RaceService : IRaceService
{
    private readonly IRaceRepository _raceRepository;

    public RaceService(IRaceRepository raceRepository)
    {
        _raceRepository = raceRepository;
    }

    public async Task<RaceScheduleResponse> CreateRaceAsync(CreateRaceRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Race name cannot be empty.", nameof(request.Name));
        }

        if (request.DistanceMeter <= 0)
        {
            throw new ArgumentException("Distance must be greater than zero.", nameof(request.DistanceMeter));
        }

        var race = new Race
        {
            RoundId = request.RoundId,
            Name = request.Name,
            RaceDate = request.RaceDate,
            DistanceMeter = request.DistanceMeter,
            MaxLanes = request.MaxLanes > 0 ? request.MaxLanes : 10,
            Status = "Scheduled"
        };

        await _raceRepository.AddAsync(race);
        await _raceRepository.SaveChangesAsync();

        var schedule = await _raceRepository.GetPublicRaceScheduleAsync();
        var savedRace = schedule.FirstOrDefault(r => r.RaceId == race.RaceId);

        return new RaceScheduleResponse
        {
            RaceId = race.RaceId,
            RoundId = race.RoundId,
            Name = race.Name,
            RaceDate = race.RaceDate,
            DistanceMeter = race.DistanceMeter,
            MaxLanes = race.MaxLanes,
            Status = race.Status,
            RoundName = savedRace?.Round?.Name ?? string.Empty,
            TournamentName = savedRace?.Round?.Tournament?.Name ?? string.Empty
        };
    }

    public async Task<List<RaceScheduleResponse>> GetPublicRaceScheduleAsync()
    {
        var races = await _raceRepository.GetPublicRaceScheduleAsync();
        return races.Select(r => new RaceScheduleResponse
        {
            RaceId = r.RaceId,
            RoundId = r.RoundId,
            Name = r.Name ?? string.Empty,
            RaceDate = r.RaceDate,
            DistanceMeter = r.DistanceMeter,
            MaxLanes = r.MaxLanes,
            Status = r.Status,
            RoundName = r.Round?.Name ?? string.Empty,
            TournamentName = r.Round?.Tournament?.Name ?? string.Empty
        }).ToList();
    }
}
