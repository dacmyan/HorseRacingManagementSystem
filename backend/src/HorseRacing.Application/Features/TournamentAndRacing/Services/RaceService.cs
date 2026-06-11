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

        var round = await _raceRepository.GetRoundByIdAsync(request.RoundId);
        if (round == null)
        {
            throw new ArgumentException("Round does not exist.", nameof(request.RoundId));
        }

        if (request.DistanceMeter <= 0)
        {
            throw new ArgumentException("Distance must be greater than zero.", nameof(request.DistanceMeter));
        }

        if (request.MaxLanes <= 0)
        {
            throw new ArgumentException("Max lanes must be greater than zero.", nameof(request.MaxLanes));
        }

        if (request.RaceDate == default)
        {
            throw new ArgumentException("Race date is invalid.", nameof(request.RaceDate));
        }

        if (round.StartDate.HasValue && round.EndDate.HasValue)
        {
            if (request.RaceDate < round.StartDate.Value || request.RaceDate > round.EndDate.Value)
            {
                throw new ArgumentException($"Race date must be between {round.StartDate.Value:yyyy-MM-dd} and {round.EndDate.Value:yyyy-MM-dd}.", nameof(request.RaceDate));
            }
        }

        var race = new Race
        {
            RoundId = request.RoundId,
            Name = request.Name,
            RaceDate = request.RaceDate,
            DistanceMeter = request.DistanceMeter,
            MaxLanes = request.MaxLanes,
            Status = "Scheduled"
        };

        await _raceRepository.AddAsync(race);
        await _raceRepository.SaveChangesAsync();

        var savedRace = await _raceRepository.GetByIdWithDetailsAsync(race.RaceId);
        if (savedRace == null)
        {
            throw new InvalidOperationException("Failed to retrieve the created race.");
        }

        return new RaceScheduleResponse
        {
            RaceId = savedRace.RaceId,
            RoundId = savedRace.RoundId,
            Name = savedRace.Name ?? string.Empty,
            RaceDate = savedRace.RaceDate,
            DistanceMeter = savedRace.DistanceMeter,
            MaxLanes = savedRace.MaxLanes,
            Status = savedRace.Status,
            RoundName = savedRace.Round?.Name ?? string.Empty,
            TournamentName = savedRace.Round?.Tournament?.Name ?? string.Empty
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
