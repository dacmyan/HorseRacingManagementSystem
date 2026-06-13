using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities;
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

    public async Task<RaceEntryResponse> CreateRaceEntryAsync(long raceId, CreateRaceEntryRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (raceId <= 0)
        {
            throw new ArgumentException("Race ID must be greater than zero.", nameof(raceId));
        }

        var race = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} not found.");
        }

        var registration = await _raceRepository.GetRegistrationWithHorseAsync(request.RegistrationId);
        if (registration == null)
        {
            throw new KeyNotFoundException($"Registration with ID {request.RegistrationId} not found.");
        }

        if (race.Round == null)
        {
            throw new InvalidOperationException("Race is not assigned to a round.");
        }

        if (registration.TournamentId != race.Round.TournamentId)
        {
            throw new InvalidOperationException("Registration tournament does not match the race tournament.");
        }

        if (request.LaneNo <= 0)
        {
            throw new ArgumentException("Lane number must be greater than zero.", nameof(request.LaneNo));
        }

        if (request.LaneNo > race.MaxLanes)
        {
            throw new InvalidOperationException($"Lane number cannot exceed the maximum of {race.MaxLanes} for this race.");
        }

        var existingEntries = await _raceRepository.GetRaceEntriesAsync(raceId);

        if (existingEntries.Any(e => e.LaneNo == request.LaneNo))
        {
            throw new InvalidOperationException($"Lane {request.LaneNo} is already occupied in this race.");
        }

        if (existingEntries.Any(e => e.RegistrationId == request.RegistrationId))
        {
            throw new InvalidOperationException("This horse registration is already entered in this race.");
        }

        if (existingEntries.Any(e => e.Registration?.HorseId == registration.HorseId))
        {
            throw new InvalidOperationException("This horse is already entered in this race.");
        }

        if (request.JockeyId.HasValue)
        {
            if (existingEntries.Any(e => e.JockeyId == request.JockeyId.Value))
            {
                throw new InvalidOperationException("This jockey is already assigned to a lane in this race.");
            }

            var hasActiveContract = await _raceRepository.HasActiveJockeyContractAsync(
                race.Round.TournamentId,
                registration.HorseId,
                request.JockeyId.Value
            );

            if (!hasActiveContract)
            {
                throw new InvalidOperationException("The jockey does not have an active contract for this horse.");
            }
        }

        var raceEntry = new RaceEntry
        {
            RaceId = raceId,
            RegistrationId = request.RegistrationId,
            JockeyId = request.JockeyId,
            LaneNo = request.LaneNo,
            WinningProbability = request.WinningProbability,
            CurrentOdds = request.CurrentOdds,
            Status = "Ready"
        };

        await _raceRepository.AddRaceEntryAsync(raceEntry);
        await _raceRepository.SaveChangesAsync();

        var jockeyName = existingEntries.FirstOrDefault(e => e.JockeyId == request.JockeyId)?.Jockey?.User?.FullName;
        if (string.IsNullOrEmpty(jockeyName) && request.JockeyId.HasValue)
        {
            var savedEntries = await _raceRepository.GetRaceEntriesAsync(raceId);
            jockeyName = savedEntries.FirstOrDefault(e => e.JockeyId == request.JockeyId)?.Jockey?.User?.FullName;
        }

        return new RaceEntryResponse
        {
            RaceEntryId = raceEntry.RaceEntryId,
            RaceId = raceEntry.RaceId,
            RegistrationId = raceEntry.RegistrationId,
            HorseId = registration.HorseId,
            HorseName = registration.Horse?.Name ?? string.Empty,
            JockeyId = raceEntry.JockeyId,
            JockeyName = jockeyName,
            LaneNo = raceEntry.LaneNo,
            Status = raceEntry.Status,
            WinningProbability = raceEntry.WinningProbability,
            CurrentOdds = raceEntry.CurrentOdds
        };
    }
}
