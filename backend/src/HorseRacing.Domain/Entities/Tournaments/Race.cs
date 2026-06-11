using System;
using System.Collections.Generic;

namespace HorseRacing.Domain.Entities.Tournaments;

public class Race
{
    public long RaceId { get; set; }
    public long RoundId { get; set; }

    // Navigation Properties
    public Round? Round { get; set; }

    public string? Name { get; set; }
    public DateTime RaceDate { get; set; }
    public int DistanceMeter { get; set; }
    public int MaxLanes { get; set; } = 10;
    public string Status { get; set; } = "Scheduled";

    // Navigation Properties
    public ICollection<RaceRefereeAssignment> RaceRefereeAssignments { get; set; } = new List<RaceRefereeAssignment>();
}
