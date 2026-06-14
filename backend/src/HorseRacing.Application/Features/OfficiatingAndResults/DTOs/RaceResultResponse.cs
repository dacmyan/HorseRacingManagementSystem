namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class RaceResultResponse
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public long? RaceEntryId { get; set; }
    public long? HorseId { get; set; }
    public string? HorseName { get; set; }
    public int? JockeyId { get; set; }
    public string? JockeyName { get; set; }
    public string Status { get; set; } = string.Empty; // Race.Status
}
