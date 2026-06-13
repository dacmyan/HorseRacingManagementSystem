using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RaceViolation
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public long RaceEntryId { get; set; }
    public RaceEntry? RaceEntry { get; set; }
    public long RefereeId { get; set; }
    public RefereeProfile? RefereeProfile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Penalty { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

