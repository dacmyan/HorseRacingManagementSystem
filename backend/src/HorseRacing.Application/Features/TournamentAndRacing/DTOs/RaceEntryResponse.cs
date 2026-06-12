namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class RaceEntryResponse
{
    public int EntryId { get; set; }
    public long RaceId { get; set; }
    public int HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public int JockeyId { get; set; }
    public string JockeyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
