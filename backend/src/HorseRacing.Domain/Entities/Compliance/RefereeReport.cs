using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RefereeReport
{
    public long ReportId { get; set; }
    
    public long AssignmentId { get; set; }
    public RaceRefereeAssignment? Assignment { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ViolationNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? ReportedUserId { get; set; }
    public AppUser? ReportedUser { get; set; }

    public long? ReportedHorseId { get; set; }
    public Horse? ReportedHorse { get; set; }
}
