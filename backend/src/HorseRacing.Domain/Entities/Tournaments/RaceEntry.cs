using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RaceEntry
{
    public long RaceEntryId { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    
    public int RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    
    public int? JockeyId { get; set; }
    public JockeyProfile? JockeyProfile { get; set; }
    
    public decimal? WinningProbability { get; set; }
    public decimal? CurrentOdds { get; set; }
    public int LaneNo { get; set; }
    public string Status { get; set; } = "Ready";
}
