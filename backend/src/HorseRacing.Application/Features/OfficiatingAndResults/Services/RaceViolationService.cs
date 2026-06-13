using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RaceViolationService : IRaceViolationService
{
    private readonly IRaceViolationRepository _repository;

    public RaceViolationService(IRaceViolationRepository repository)
    {
        _repository = repository;
    }

    public async Task<RaceViolationResponse> CreateViolationAsync(long raceId, CreateRaceViolationRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 1. raceId must exist
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        // 2. description is required
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Description is required.", nameof(request.Description));
        }

        // 3. refereeId must exist
        var referee = await _repository.GetRefereeProfileByIdAsync(request.RefereeId);
        if (referee == null)
        {
            throw new KeyNotFoundException($"Referee with ID {request.RefereeId} was not found.");
        }

        // 4. referee must be assigned to the race through RaceRefereeAssignment
        var assignment = await _repository.GetAssignmentAsync(raceId, request.RefereeId);
        if (assignment == null)
        {
            throw new ArgumentException($"Referee with ID {request.RefereeId} is not assigned to race with ID {raceId}.");
        }

        // 5. raceEntryId must exist
        var raceEntry = await _repository.GetRaceEntryByIdAsync(request.RaceEntryId);
        if (raceEntry == null)
        {
            throw new KeyNotFoundException($"Race entry with ID {request.RaceEntryId} was not found.");
        }

        // 6. raceEntryId must belong to the given raceId
        if (raceEntry.RaceId != raceId)
        {
            throw new ArgumentException($"Race entry with ID {request.RaceEntryId} does not belong to race with ID {raceId}.");
        }

        // 7. Create violation and set CreatedAt
        var violation = new RaceViolation
        {
            RaceId = raceId,
            RaceEntryId = request.RaceEntryId,
            RefereeId = request.RefereeId,
            Description = request.Description,
            Penalty = request.Penalty ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddViolationAsync(violation);
        await _repository.SaveChangesAsync();

        // 8. Map entity to RaceViolationResponse
        return new RaceViolationResponse
        {
            Id = violation.Id,
            RaceId = violation.RaceId,
            RaceEntryId = violation.RaceEntryId,
            RefereeId = violation.RefereeId,
            Description = violation.Description,
            Penalty = violation.Penalty,
            CreatedAt = violation.CreatedAt
        };
    }

    public async Task<List<RaceViolationResponse>> GetViolationsByRaceIdAsync(long raceId)
    {
        // 1. raceId must exist
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        var violations = await _repository.GetViolationsByRaceIdAsync(raceId);

        return violations.Select(v => new RaceViolationResponse
        {
            Id = v.Id,
            RaceId = v.RaceId,
            RaceEntryId = v.RaceEntryId,
            RefereeId = v.RefereeId,
            Description = v.Description,
            Penalty = v.Penalty,
            CreatedAt = v.CreatedAt
        }).ToList();
    }
}
