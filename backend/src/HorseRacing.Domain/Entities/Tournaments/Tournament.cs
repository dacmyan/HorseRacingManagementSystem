using System;
using System.Collections.Generic;

namespace HorseRacing.Domain.Entities.Tournaments;

public class Tournament
{
    public long TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Upcoming";

    // Navigation Properties
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
