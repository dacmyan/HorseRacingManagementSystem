using System;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class RoundRaceResponse
{
    public long RaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RaceDate { get; set; }
    public int DistanceMeter { get; set; }
    public int MaxLanes { get; set; }
    public string Status { get; set; } = "Scheduled";
}
