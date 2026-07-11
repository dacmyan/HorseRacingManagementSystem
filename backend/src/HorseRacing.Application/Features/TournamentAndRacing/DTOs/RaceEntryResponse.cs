namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class RaceEntryResponse
{
    public long RaceEntryId { get; set; }
    public long RaceId { get; set; }
    public long RegistrationId { get; set; }
    public long HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public int? JockeyId { get; set; }
    public string? JockeyName { get; set; }
    public int LaneNo { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public decimal? WinningProbability { get; set; }
    public decimal? CurrentOdds { get; set; }
    public int? FinishPosition { get; set; }
    public decimal? FinishTime { get; set; }
}
