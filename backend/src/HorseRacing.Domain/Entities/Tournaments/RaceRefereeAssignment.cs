using System;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Domain.Entities.Tournaments;

public class RaceRefereeAssignment
{
    public long AssignmentId { get; set; }
    public long RaceId { get; set; }

    // Navigation Properties
    public Race? Race { get; set; }

    public long RefereeId { get; set; }

    // Navigation property to RefereeProfile
    public RefereeProfile? RefereeProfile { get; set; }

    public DateTime? AssignedAt { get; set; }
    public string Status { get; set; } = "Active";
}
