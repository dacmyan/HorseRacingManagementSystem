using System;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class RaceScheduleResponse
{
    public long RaceId { get; set; }
    public long RoundId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RaceDate { get; set; }
    public int DistanceMeter { get; set; }
    public int MaxLanes { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string RoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public long TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public bool HasHealthIssue { get; set; }
}
