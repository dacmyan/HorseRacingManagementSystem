using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.OfficiatingAndResults.Services;

public class RefereeService : IRefereeService
{
    private readonly IViolationRepository _repository;
    private readonly IRefereeReportRepository _reportRepository;

    public RefereeService(IViolationRepository repository, IRefereeReportRepository reportRepository)
    {
        _repository = repository;
        _reportRepository = reportRepository;
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

        if (string.Equals(race.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) && race.RaceDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot record violations for a race that has not started yet.");
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
        if (!string.Equals(assignment.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The referee assignment is no longer active.");

        // 4. Validate description presence
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Violation description cannot be empty.", nameof(request.Description));
        }
        request.Description = request.Description.Trim();
        request.Penalty = request.Penalty?.Trim() ?? string.Empty;
        if (request.Description.Length > 1000)
            throw new ArgumentException("Violation description cannot exceed 1000 characters.", nameof(request.Description));
        var allowedPenalties = new[] { "None", "Time Penalty", "Disqualified" };
        if (!allowedPenalties.Contains(request.Penalty, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Penalty must be one of: {string.Join(", ", allowedPenalties)}.", nameof(request.Penalty));

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

    public async Task<RefereeReportResponse> SubmitReportAsync(CreateRefereeReportRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 1. Validate assignment existence (either via AssignmentId or RaceId + RefereeId)
        RaceRefereeAssignment? assignment = null;
        if (request.AssignmentId.HasValue && request.AssignmentId.Value > 0)
        {
            assignment = await _reportRepository.GetAssignmentByIdAsync(request.AssignmentId.Value);
            if (assignment == null)
            {
                throw new KeyNotFoundException($"Race referee assignment with ID {request.AssignmentId.Value} was not found.");
            }
        }
        else if (request.RaceId.HasValue && request.RefereeId.HasValue)
        {
            assignment = await _reportRepository.GetAssignmentByRaceAndRefereeAsync(request.RaceId.Value, request.RefereeId.Value);
            if (assignment == null)
            {
                throw new KeyNotFoundException($"Race referee assignment for Race ID {request.RaceId.Value} and Referee ID {request.RefereeId.Value} was not found.");
            }
        }
        else
        {
            throw new ArgumentException("Either AssignmentId or both RaceId and RefereeId must be provided.");
        }

        if (assignment.Race != null && string.Equals(assignment.Race.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) && assignment.Race.RaceDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot submit a report for a race that has not started yet.");
        }
        if (!string.Equals(assignment.Status, "Active", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cannot submit a report because the referee assignment is no longer active.");

        // 2. Validate referee user role
        if (assignment.RefereeProfile == null)
        {
            throw new InvalidOperationException("Assignment does not have an associated referee profile.");
        }

        if (assignment.RefereeProfile.User == null || assignment.RefereeProfile.User.Role == null || !string.Equals(assignment.RefereeProfile.User.Role.Name, "Referee", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The assigned referee profile is not associated with a valid Referee user role.");
        }

        // 3. Validate content presence
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Report content cannot be empty.", nameof(request.Content));
        }
        request.Content = request.Content.Trim();
        request.ViolationNote = request.ViolationNote?.Trim();
        if (request.Content.Length > 2000)
            throw new ArgumentException("Report content cannot exceed 2000 characters.", nameof(request.Content));
        if (request.ViolationNote?.Length > 1000)
            throw new ArgumentException("Violation note cannot exceed 1000 characters.", nameof(request.ViolationNote));

        // 4. Validate ReportedUserId if provided
        if (request.ReportedUserId.HasValue)
        {
            var userExists = await _reportRepository.UserExistsAsync(request.ReportedUserId.Value);
            if (!userExists)
            {
                throw new KeyNotFoundException($"Reported user with ID {request.ReportedUserId.Value} was not found.");
            }
        }

        // 5. Validate ReportedHorseId if provided
        if (request.ReportedHorseId.HasValue)
        {
            var horseExists = await _reportRepository.HorseExistsAsync(request.ReportedHorseId.Value);
            if (!horseExists)
            {
                throw new KeyNotFoundException($"Reported horse with ID {request.ReportedHorseId.Value} was not found.");
            }
        }

        // 6. Create report
        var report = new RefereeReport
        {
            AssignmentId = assignment.AssignmentId,
            Content = request.Content,
            ViolationNote = request.ViolationNote,
            ReportedUserId = request.ReportedUserId,
            ReportedHorseId = request.ReportedHorseId,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddReportAsync(report);
        await _reportRepository.SaveChangesAsync();

        return new RefereeReportResponse
        {
            ReportId = report.ReportId,
            AssignmentId = report.AssignmentId,
            RaceId = assignment.RaceId,
            RaceName = assignment.Race?.Name ?? string.Empty,
            RefereeId = assignment.RefereeId,
            RefereeName = assignment.RefereeProfile?.User?.FullName ?? "Unknown Referee",
            Content = report.Content,
            ViolationNote = report.ViolationNote,
            ReportedUserId = report.ReportedUserId,
            ReportedHorseId = report.ReportedHorseId,
            CreatedAt = report.CreatedAt
        };
    }

    public async Task<List<RefereeReportResponse>?> GetReportsByRaceIdAsync(long raceId)
    {
        var raceExists = await _reportRepository.RaceExistsAsync(raceId);
        if (!raceExists)
        {
            return null;
        }

        var reports = await _reportRepository.GetReportsByRaceIdAsync(raceId);
        return reports.Select(r => new RefereeReportResponse
        {
            ReportId = r.ReportId,
            AssignmentId = r.AssignmentId,
            RaceId = r.Assignment?.RaceId ?? 0,
            RaceName = r.Assignment?.Race?.Name ?? string.Empty,
            RefereeId = r.Assignment?.RefereeId ?? 0,
            RefereeName = r.Assignment?.RefereeProfile?.User?.FullName ?? "Unknown Referee",
            Content = r.Content,
            ViolationNote = r.ViolationNote,
            ReportedUserId = r.ReportedUserId,
            ReportedHorseId = r.ReportedHorseId,
            ReportedHorseName = r.ReportedHorse?.Name,
            CreatedAt = r.CreatedAt
        }).ToList();
    }
}
