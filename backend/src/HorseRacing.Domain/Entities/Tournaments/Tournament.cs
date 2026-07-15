using System;
using System.Collections.Generic;

namespace HorseRacing.Domain.Entities.Tournaments;

public class Tournament
{
    public long TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Registration Open";
    public int CancelCount { get; set; } = 0;

    // Navigation Properties
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
