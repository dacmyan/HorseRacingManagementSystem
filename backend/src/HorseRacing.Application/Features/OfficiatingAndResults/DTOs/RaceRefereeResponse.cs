using System;

namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class RaceRefereeResponse
{
    public long AssignmentId { get; set; }
    public long RaceId { get; set; }
    public int RefereeId { get; set; }
    public string RefereeName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? AssignedAt { get; set; }
}
