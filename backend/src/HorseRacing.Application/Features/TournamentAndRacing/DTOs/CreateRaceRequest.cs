using System;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateRaceRequest
{
    public long RoundId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RaceDate { get; set; }
    public int DistanceMeter { get; set; }
    public int MaxLanes { get; set; }
}
