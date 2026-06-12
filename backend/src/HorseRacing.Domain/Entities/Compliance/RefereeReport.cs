using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RefereeReport
{
    public int Id { get; set; }
    
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    
    public int RefereeId { get; set; }
    public RefereeProfile? RefereeProfile { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Các cột báo cáo vi phạm tùy chọn
    public int? ReportedUserId { get; set; }
    public AppUser? ReportedUser { get; set; }

    public int? ReportedHorseId { get; set; }
    public Horse? ReportedHorse { get; set; }
}
