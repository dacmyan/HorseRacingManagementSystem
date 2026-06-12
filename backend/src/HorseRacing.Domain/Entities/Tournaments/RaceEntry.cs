using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RaceEntry
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public int HorseId { get; set; }
    public Horse? Horse { get; set; }
    public int JockeyId { get; set; }
    public AppUser? Jockey { get; set; }
    public int LaneNo { get; set; }
    public string Status { get; set; } = string.Empty;
}

