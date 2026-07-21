using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.Notifications.Interfaces;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RefereeAssignmentService : IRefereeAssignmentService
{
    private readonly IRefereeAssignmentRepository _repository;
    private readonly INotificationService _notificationService;

    public RefereeAssignmentService(
        IRefereeAssignmentRepository repository,
        INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<RaceRefereeResponse> AssignRefereeAsync(long raceId, AssignRefereeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (raceId <= 0 || request.RefereeId <= 0)
            throw new ArgumentException("Race ID and referee ID must be greater than zero.");

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
        if (!string.Equals(refereeProfile.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(refereeProfile.User.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only an active referee with an active user account can be assigned.");
        if (new[] { "Live", "InProgress", "Running", "Finished", "Completed", "Cancelled" }
            .Contains(race.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Referees cannot be assigned while race status is '{race.Status}'.");

        // 3. Check duplicate assignment
        var existing = await _repository.GetAssignmentAsync(raceId, request.RefereeId);
        if (existing != null)
        {
            throw new InvalidOperationException("Referee is already assigned to this race.");
        }
        if (await _repository.HasScheduleConflictAsync(request.RefereeId, raceId, race.RaceDate))
            throw new InvalidOperationException("This referee is already assigned to another race at the same time.");

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

        // Send notification to referee
        try
        {
            await _notificationService.SendNotificationToUserAsync(
                refereeProfile.UserId,
                "New Officiating Assignment",
                $"You have been assigned to officiate race '{race.Name}' scheduled on {race.RaceDate:dd/MM/yyyy HH:mm}.",
                "System",
                referenceId: (int)raceId,
                actionUrl: "/referee/schedule"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send referee assignment notification: {ex.Message}");
        }

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
        if (new[] { "Live", "InProgress", "Running", "Finished", "Completed", "Cancelled" }
            .Contains(race.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Referee assignments cannot be removed while race status is '{race.Status}'.");

        _repository.RemoveAssignment(assignment);
        await _repository.SaveChangesAsync();

        try
        {
            await _notificationService.SendNotificationToUserAsync(
                referee.UserId,
                "Officiating assignment removed",
                $"Your assignment for race '{race.Name}' scheduled on {race.RaceDate:dd/MM/yyyy HH:mm} has been removed.",
                "Race",
                referenceId: (int)raceId,
                actionUrl: "/referee/schedule");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send referee removal notification: {ex.Message}");
        }
    }
}
