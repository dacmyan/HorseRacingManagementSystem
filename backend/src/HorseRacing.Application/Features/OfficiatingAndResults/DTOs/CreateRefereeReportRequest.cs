namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class CreateRefereeReportRequest
{
    public long? AssignmentId { get; set; }
    public long? RaceId { get; set; }
    public int? RefereeId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ViolationNote { get; set; }
    public int? ReportedUserId { get; set; }
    public long? ReportedHorseId { get; set; }
}
