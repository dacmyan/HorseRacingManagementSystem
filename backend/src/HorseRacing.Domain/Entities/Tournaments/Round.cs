using System;
using System.Collections.Generic;

namespace HorseRacing.Domain.Entities.Tournaments;

public class Round
{
    public long RoundId { get; set; }
    public long TournamentId { get; set; }

    // Navigation Properties
    public Tournament? Tournament { get; set; }

    public string? Name { get; set; }
    public int RoundNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Scheduled";

    // Navigation Properties
    public ICollection<Race> Races { get; set; } = new List<Race>();
}
