using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RaceResultService : IRaceResultService
{
    private readonly IResultRepository _repository;

    public RaceResultService(IResultRepository repository)
    {
        _repository = repository;
    }

    public async Task<RaceResultResponse> SubmitResultAsync(SubmitRaceResultRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 1. Validate race existence
        var race = await _repository.GetRaceByIdAsync(request.RaceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {request.RaceId} was not found.");
        }

        // 2. Validate referee assignment if RefereeId is provided
        if (request.RefereeId.HasValue)
        {
            var assignment = await _repository.GetAssignmentAsync(request.RaceId, request.RefereeId.Value);
            if (assignment == null)
            {
                throw new InvalidOperationException("The referee is not assigned to this race.");
            }
        }

        // 3. Validate Winner parameter
        if (string.IsNullOrWhiteSpace(request.Winner))
        {
            throw new ArgumentException("Winner identifier cannot be empty.", nameof(request.Winner));
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(request.Winner);
        if (horse == null)
        {
            throw new KeyNotFoundException($"Winner horse '{request.Winner}' was not found.");
        }

        // 4. Validate that the horse is entered in the race
        var entry = await _repository.GetRaceEntryByHorseIdAsync(request.RaceId, horse.HorseId);
        if (entry == null)
        {
            throw new ArgumentException($"Horse '{horse.Name}' is not entered in race with ID {request.RaceId}.");
        }

        // 5. Prevent duplicate result submission for the same race
        var existingResult = await _repository.GetResultByRaceIdAsync(request.RaceId);
        if (existingResult != null)
        {
            throw new InvalidOperationException($"A result has already been submitted for race with ID {request.RaceId}.");
        }

        // 6. Save result (using only actual properties from DB/entity)
        var result = new RaceResult
        {
            RaceId = request.RaceId,
            Winner = request.Winner
        };

        await _repository.AddResultAsync(result);
        await _repository.SaveChangesAsync();

        return new RaceResultResponse
        {
            Id = result.Id,
            RaceId = result.RaceId,
            RaceName = race.Name ?? string.Empty,
            Winner = result.Winner,
            RaceEntryId = entry.RaceEntryId,
            HorseId = horse.HorseId,
            HorseName = horse.Name,
            JockeyId = entry.JockeyId,
            JockeyName = entry.JockeyProfile?.User?.FullName ?? string.Empty,
            Status = race.Status
        };
    }

    public async Task<RaceResultResponse> PublishResultAsync(long raceId)
    {
        // 1. Validate race existence
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        // 2. Validate result presence
        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            throw new InvalidOperationException($"No results found for race with ID {raceId}. Cannot publish.");
        }

        // 3. Update race status to "Finished"
        race.Status = "Finished";
        await _repository.SaveChangesAsync();

        // 4. Resolve winner details
        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        return new RaceResultResponse
        {
            Id = result.Id,
            RaceId = result.RaceId,
            RaceName = race.Name ?? string.Empty,
            Winner = result.Winner,
            RaceEntryId = entry?.RaceEntryId,
            HorseId = horse?.HorseId,
            HorseName = horse?.Name,
            JockeyId = entry?.JockeyId,
            JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
            Status = race.Status
        };
    }

    public async Task<List<RaceResultResponse>?> GetResultsByRaceIdAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            return null;
        }

        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null)
        {
            return new List<RaceResultResponse>();
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        return new List<RaceResultResponse>
        {
            new RaceResultResponse
            {
                Id = result.Id,
                RaceId = result.RaceId,
                RaceName = race.Name ?? string.Empty,
                Winner = result.Winner,
                RaceEntryId = entry?.RaceEntryId,
                HorseId = horse?.HorseId,
                HorseName = horse?.Name,
                JockeyId = entry?.JockeyId,
                JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
                Status = race.Status
            }
        };
    }

    public async Task<List<RaceResultResponse>?> GetPublicResultsByRaceIdAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            return null;
        }

        // Public should only see published results (Status == "Finished")
        if (race.Status != "Finished")
        {
            return new List<RaceResultResponse>();
        }

        var result = await _repository.GetResultByRaceIdAsync(raceId);
        if (result == null)
        {
            return new List<RaceResultResponse>();
        }

        var horse = await _repository.GetHorseByIdOrNameAsync(result.Winner);
        RaceEntry? entry = null;
        if (horse != null)
        {
            entry = await _repository.GetRaceEntryByHorseIdAsync(raceId, horse.HorseId);
        }

        return new List<RaceResultResponse>
        {
            new RaceResultResponse
            {
                Id = result.Id,
                RaceId = result.RaceId,
                RaceName = race.Name ?? string.Empty,
                Winner = result.Winner,
                RaceEntryId = entry?.RaceEntryId,
                HorseId = horse?.HorseId,
                HorseName = horse?.Name,
                JockeyId = entry?.JockeyId,
                JockeyName = entry?.JockeyProfile?.User?.FullName ?? string.Empty,
                Status = race.Status
            }
        };
    }
}
