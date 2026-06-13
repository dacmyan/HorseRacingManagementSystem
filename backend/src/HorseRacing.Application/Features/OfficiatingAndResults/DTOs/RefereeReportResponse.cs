using System;

namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class RefereeReportResponse
{
    public long ReportId { get; set; }
    public long AssignmentId { get; set; }
    public long RaceId { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public int RefereeId { get; set; }
    public string RefereeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ViolationNote { get; set; }
    public int? ReportedUserId { get; set; }
    public long? ReportedHorseId { get; set; }
    public DateTime CreatedAt { get; set; }
}
