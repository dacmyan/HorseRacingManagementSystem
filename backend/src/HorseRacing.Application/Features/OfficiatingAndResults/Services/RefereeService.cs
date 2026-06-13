using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RefereeService : IRefereeService
{
    private readonly IViolationRepository _repository;

    public RefereeService(IViolationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ViolationResponse> LogViolationAsync(LogViolationRequest request)
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

        // 2. Validate referee profile existence and role
        var refereeProfile = await _repository.GetRefereeProfileByIdAsync(request.RefereeId);
        if (refereeProfile == null)
        {
            throw new KeyNotFoundException($"Referee with ID {request.RefereeId} was not found.");
        }

        if (refereeProfile.User == null || refereeProfile.User.Role == null || !string.Equals(refereeProfile.User.Role.Name, "Referee", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified referee profile is not associated with a valid Referee user role.");
        }

        // 3. Verify referee is assigned to the target race
        var assignment = await _repository.GetAssignmentAsync(request.RaceId, request.RefereeId);
        if (assignment == null)
        {
            throw new InvalidOperationException("The referee is not assigned to this race.");
        }

        // 4. Validate description presence
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Violation description cannot be empty.", nameof(request.Description));
        }

        // 5. Create violation
        var violation = new RaceViolation
        {
            RaceId = request.RaceId,
            Description = request.Description,
            Penalty = request.Penalty ?? string.Empty
        };

        await _repository.AddViolationAsync(violation);
        await _repository.SaveChangesAsync();

        return new ViolationResponse
        {
            ViolationId = violation.Id,
            RaceId = violation.RaceId,
            RaceName = race.Name ?? string.Empty,
            Description = violation.Description,
            Penalty = violation.Penalty,
            RefereeId = refereeProfile.RefereeId,
            RefereeName = refereeProfile.User?.FullName ?? "Unknown Referee"
        };
    }

    public async Task<List<ViolationResponse>?> GetViolationsByRaceIdAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            return null;
        }

        var violations = await _repository.GetViolationsByRaceIdAsync(raceId);
        return violations.Select(v => new ViolationResponse
        {
            ViolationId = v.Id,
            RaceId = v.RaceId,
            RaceName = v.Race?.Name ?? string.Empty,
            Description = v.Description,
            Penalty = v.Penalty
        }).ToList();
    }
}
