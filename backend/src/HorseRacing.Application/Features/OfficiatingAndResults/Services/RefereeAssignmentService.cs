using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RefereeAssignmentService : IRefereeAssignmentService
{
    private readonly IRefereeAssignmentRepository _repository;

    public RefereeAssignmentService(IRefereeAssignmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<RaceRefereeResponse> AssignRefereeAsync(long raceId, AssignRefereeRequest request)
    {
        // 1. Validate race existence
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        // 2. Validate referee existence and role
        var refereeProfile = await _repository.GetRefereeProfileByIdAsync(request.RefereeId);
        if (refereeProfile == null)
        {
            throw new KeyNotFoundException($"Referee with ID {request.RefereeId} was not found.");
        }

        if (refereeProfile.User == null || refereeProfile.User.Role == null || !string.Equals(refereeProfile.User.Role.Name, "Referee", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified referee profile is not associated with a valid Referee user role.");
        }

        // 3. Check duplicate assignment
        var existing = await _repository.GetAssignmentAsync(raceId, request.RefereeId);
        if (existing != null)
        {
            throw new InvalidOperationException("Referee is already assigned to this race.");
        }

        // 4. Create and add assignment
        var assignment = new RaceRefereeAssignment
        {
            RaceId = raceId,
            RefereeId = request.RefereeId,
            AssignedAt = DateTime.UtcNow,
            Status = "Active"
        };

        await _repository.AddAssignmentAsync(assignment);
        await _repository.SaveChangesAsync();

        // Load details for response (including user name)
        var refereeName = refereeProfile.User?.FullName ?? "Unknown Referee";

        return new RaceRefereeResponse
        {
            AssignmentId = assignment.AssignmentId,
            RaceId = assignment.RaceId,
            RefereeId = assignment.RefereeId,
            RefereeName = refereeName,
            LicenseNumber = refereeProfile.LicenseNumber,
            ExperienceYears = refereeProfile.ExperienceYears,
            Status = assignment.Status,
            AssignedAt = assignment.AssignedAt
        };
    }

    public async Task<List<RaceRefereeResponse>> GetAssignedRefereesAsync(long raceId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        var assignments = await _repository.GetAssignmentsForRaceAsync(raceId);

        return assignments.Select(a => new RaceRefereeResponse
        {
            AssignmentId = a.AssignmentId,
            RaceId = a.RaceId,
            RefereeId = a.RefereeId,
            RefereeName = a.RefereeProfile?.User?.FullName ?? "Unknown Referee",
            LicenseNumber = a.RefereeProfile?.LicenseNumber ?? string.Empty,
            ExperienceYears = a.RefereeProfile?.ExperienceYears ?? 0,
            Status = a.Status,
            AssignedAt = a.AssignedAt
        }).ToList();
    }

    public async Task RemoveRefereeAssignmentAsync(long raceId, int refereeId)
    {
        var race = await _repository.GetRaceByIdAsync(raceId);
        if (race == null)
        {
            throw new KeyNotFoundException($"Race with ID {raceId} was not found.");
        }

        var referee = await _repository.GetRefereeProfileByIdAsync(refereeId);
        if (referee == null)
        {
            throw new KeyNotFoundException($"Referee with ID {refereeId} was not found.");
        }

        var assignment = await _repository.GetAssignmentAsync(raceId, refereeId);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"Referee assignment not found for Race ID {raceId} and Referee ID {refereeId}.");
        }

        _repository.RemoveAssignment(assignment);
        await _repository.SaveChangesAsync();
    }
}
