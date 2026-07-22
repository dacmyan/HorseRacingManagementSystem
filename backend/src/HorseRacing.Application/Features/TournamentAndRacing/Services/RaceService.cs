using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class RaceService : IRaceService
{
    private readonly IRaceRepository _raceRepository;
    private readonly IBettingService _bettingService;

    public RaceService(IRaceRepository raceRepository, IBettingService bettingService)
    {
        _raceRepository = raceRepository;
        _bettingService = bettingService;
    }

    private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

    public async Task<RaceScheduleResponse> CreateRaceAsync(CreateRaceRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Race name cannot be empty.", nameof(request.Name));
        }
        if (request.Name.Length > 150)
            throw new ArgumentException("Race name cannot exceed 150 characters.", nameof(request.Name));

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

        if (request.MaxLanes > 12)
        {
            throw new ArgumentException("Max lanes cannot exceed 12.", nameof(request.MaxLanes));
        }

        if (request.RaceDate == default)
        {
            throw new ArgumentException("Race date is invalid.", nameof(request.RaceDate));
        }
        if (request.RaceDate < VietnamNow.AddMinutes(-5))
            throw new ArgumentException("Race date cannot be in the past.", nameof(request.RaceDate));

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
            RoundNumber = savedRace.Round?.RoundNumber ?? 0,
            TournamentId = savedRace.Round?.TournamentId ?? 0,
            TournamentName = savedRace.Round?.Tournament?.Name ?? string.Empty
        };
    }

    public async Task<List<RaceScheduleResponse>> GetPublicRaceScheduleAsync()
    {
        var races = await _raceRepository.GetPublicRaceScheduleAsync();
        var raceIds = races.Select(r => r.RaceId).ToList();
        var issueRaceIds = await _raceRepository.GetRaceIdsWithHealthIssuesAsync(raceIds);

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
            RoundNumber = r.Round?.RoundNumber ?? 0,
            TournamentId = r.Round?.TournamentId ?? 0,
            TournamentName = r.Round?.Tournament?.Name ?? string.Empty,
            HasHealthIssue = issueRaceIds.Contains(r.RaceId)
        }).ToList();
    }

    public async Task<RaceScheduleResponse?> GetRaceByIdAsync(long raceId)
    {
        var race = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (race == null)
        {
            return null;
        }

        var issueRaceIds = await _raceRepository.GetRaceIdsWithHealthIssuesAsync(new[] { race.RaceId });

        return new RaceScheduleResponse
        {
            RaceId = race.RaceId,
            RoundId = race.RoundId,
            Name = race.Name ?? string.Empty,
            RaceDate = race.RaceDate,
            DistanceMeter = race.DistanceMeter,
            MaxLanes = race.MaxLanes,
            Status = race.Status,
            RoundName = race.Round?.Name ?? string.Empty,
            RoundNumber = race.Round?.RoundNumber ?? 0,
            TournamentId = race.Round?.TournamentId ?? 0,
            TournamentName = race.Round?.Tournament?.Name ?? string.Empty,
            HasHealthIssue = issueRaceIds.Contains(race.RaceId)
        };
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

        if (!string.Equals(registration.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Approved registrations can be assigned to a race.");
        if (registration.Horse == null)
            throw new InvalidOperationException("Registration is not associated with a horse.");
        if (new[] { "Sick", "Injured" }.Contains(registration.Horse.HealthStatus, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("A sick or injured horse cannot be assigned to a race.");
        var latestMedicalCheck = registration.MedicalCheckRecords?.OrderByDescending(m => m.CheckedAt).FirstOrDefault();
        if (latestMedicalCheck == null ||
            !(string.Equals(latestMedicalCheck.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(latestMedicalCheck.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase)) ||
            string.Equals(latestMedicalCheck.DopingResult, "Positive", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Horse must pass its latest medical and doping check before lane assignment.");
        if (new[] { "Live", "InProgress", "Running", "Finished", "Completed", "Cancelled" }
            .Contains(race.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Race entries cannot be changed while race status is '{race.Status}'.");
        if (request.WinningProbability is < 0 or > 100)
            throw new ArgumentException("Winning probability must be between 0 and 100.", nameof(request.WinningProbability));
        if (request.CurrentOdds.HasValue && request.CurrentOdds <= 1)
            throw new ArgumentException("Current odds must be greater than 1.", nameof(request.CurrentOdds));

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
        if (await _raceRepository.HasHorseScheduleConflictAsync(registration.HorseId, raceId, race.RaceDate))
            throw new InvalidOperationException("This horse is already entered in another race at the same time.");

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
            if (await _raceRepository.HasJockeyScheduleConflictAsync(request.JockeyId.Value, raceId, race.RaceDate))
                throw new InvalidOperationException("This jockey is already assigned to another race at the same time.");
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

        var jockeyName = existingEntries.FirstOrDefault(e => e.JockeyId == request.JockeyId)?.JockeyProfile?.User?.FullName;
        if (string.IsNullOrEmpty(jockeyName) && request.JockeyId.HasValue)
        {
            var savedEntries = await _raceRepository.GetRaceEntriesAsync(raceId);
            jockeyName = savedEntries.FirstOrDefault(e => e.JockeyId == request.JockeyId)?.JockeyProfile?.User?.FullName;
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
            HealthStatus = registration.Horse?.HealthStatus ?? "Healthy",
            WinningProbability = raceEntry.WinningProbability,
            CurrentOdds = raceEntry.CurrentOdds
        };
    }

    public async Task<List<RaceEntryResponse>?> GetRaceEntriesByRaceIdAsync(long raceId)
    {
        var raceExists = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (raceExists == null)
        {
            return null;
        }
        if (raceExists.Round == null)
        {
            throw new InvalidOperationException("Race is missing round information.");
        }

        try
        {
            await _bettingService.RecalculateRaceOddsAsync(raceId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ODDS RECALC ERROR] {ex.Message}");
        }

        var entries = await _raceRepository.GetRaceEntriesAsync(raceId);
        var responses = new List<RaceEntryResponse>();
        foreach (var entry in entries.OrderBy(e => e.LaneNo))
        {
            int? jockeyId = entry.JockeyId;
            var jockeyName = entry.JockeyProfile?.User?.FullName ?? string.Empty;

            if (!jockeyId.HasValue && entry.Registration != null)
            {
                var activeJockey = await _raceRepository.GetActiveJockeyForHorseAsync(
                    raceExists.Round.TournamentId,
                    entry.Registration.HorseId);

                if (activeJockey.HasValue)
                {
                    jockeyId = activeJockey.Value.JockeyProfileId;
                    jockeyName = activeJockey.Value.JockeyName;
                }
            }

            responses.Add(new RaceEntryResponse
            {
                RaceEntryId = entry.RaceEntryId,
                RaceId = entry.RaceId,
                RegistrationId = entry.RegistrationId,
                HorseId = entry.Registration?.HorseId ?? 0,
                HorseName = entry.Registration?.Horse?.Name ?? string.Empty,
                JockeyId = jockeyId,
                JockeyName = jockeyName,
                LaneNo = entry.LaneNo,
                Status = entry.Status,
                HealthStatus = entry.Registration?.Horse?.HealthStatus ?? "Healthy",
                WinningProbability = entry.WinningProbability,
                CurrentOdds = entry.CurrentOdds,
                FinishPosition = entry.FinishPosition,
                FinishTime = entry.FinishTime
            });
        }

        return responses;
    }

    public async Task DeleteRaceAsync(long raceId)
    {
        if (raceId <= 0)
        {
            throw new ArgumentException("Race ID must be greater than zero.", nameof(raceId));
        }

        var race = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} not found.");
        }

        if (new[] { "Live", "InProgress", "Running", "Finished", "Completed" }
            .Contains(race.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Race cannot be deleted while its status is '{race.Status}'.");
        if (await _raceRepository.HasFinancialOrResultDataAsync(raceId))
            throw new InvalidOperationException("Race cannot be deleted after bets or results have been recorded.");

        await _raceRepository.DeleteRaceAsync(raceId);
    }

    public async Task<RaceScheduleResponse?> UpdateRaceAsync(long raceId, UpdateRaceRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Race name cannot be empty.", nameof(request.Name));
        }
        if (request.Name.Length > 150)
            throw new ArgumentException("Race name cannot exceed 150 characters.", nameof(request.Name));

        var race = await _raceRepository.GetByIdWithDetailsAsync(raceId);
        if (race == null)
        {
            return null;
        }

        if (new[] { "Live", "InProgress", "Running", "Finished", "Completed", "Cancelled" }
            .Contains(race.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Race cannot be edited while its status is '{race.Status}'.");
        }

        if (request.DistanceMeter <= 0)
        {
            throw new ArgumentException("Distance must be greater than zero.", nameof(request.DistanceMeter));
        }

        if (request.MaxLanes <= 0)
        {
            throw new ArgumentException("Max lanes must be greater than zero.", nameof(request.MaxLanes));
        }

        if (request.MaxLanes > 12)
        {
            throw new ArgumentException("Max lanes cannot exceed 12.", nameof(request.MaxLanes));
        }

        if (request.RaceDate == default)
        {
            throw new ArgumentException("Race date is invalid.", nameof(request.RaceDate));
        }
        if (request.RaceDate < VietnamNow.AddMinutes(-5))
            throw new ArgumentException("Race date cannot be in the past.", nameof(request.RaceDate));

        var round = await _raceRepository.GetRoundByIdAsync(race.RoundId);
        if (round != null && round.StartDate.HasValue && round.EndDate.HasValue)
        {
            if (request.RaceDate < round.StartDate.Value || request.RaceDate > round.EndDate.Value)
            {
                throw new ArgumentException($"Race date must be between {round.StartDate.Value:yyyy-MM-dd} and {round.EndDate.Value:yyyy-MM-dd}.", nameof(request.RaceDate));
            }
        }

        var existingEntries = await _raceRepository.GetRaceEntriesAsync(raceId);
        if (existingEntries != null && existingEntries.Any())
        {
            var maxOccupiedLane = existingEntries.Max(e => e.LaneNo);
            if (request.MaxLanes < maxOccupiedLane)
            {
                throw new InvalidOperationException($"Cannot set max lanes to {request.MaxLanes} because lane {maxOccupiedLane} is already occupied.");
            }
        }

        race.Name = request.Name;
        race.RaceDate = request.RaceDate;
        race.DistanceMeter = request.DistanceMeter;
        race.MaxLanes = request.MaxLanes;

        await _raceRepository.SaveChangesAsync();

        var issueRaceIds = await _raceRepository.GetRaceIdsWithHealthIssuesAsync(new[] { race.RaceId });

        return new RaceScheduleResponse
        {
            RaceId = race.RaceId,
            RoundId = race.RoundId,
            Name = race.Name ?? string.Empty,
            RaceDate = race.RaceDate,
            DistanceMeter = race.DistanceMeter,
            MaxLanes = race.MaxLanes,
            Status = race.Status,
            RoundName = race.Round?.Name ?? string.Empty,
            RoundNumber = race.Round?.RoundNumber ?? 0,
            TournamentId = race.Round?.TournamentId ?? 0,
            TournamentName = race.Round?.Tournament?.Name ?? string.Empty,
            HasHealthIssue = issueRaceIds.Contains(race.RaceId)
        };
    }
}
