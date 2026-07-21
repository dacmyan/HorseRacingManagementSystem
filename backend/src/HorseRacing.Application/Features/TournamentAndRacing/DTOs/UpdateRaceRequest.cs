using System;
using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class UpdateRaceRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public DateTime RaceDate { get; set; }

    [Range(1, 100000)]
    public int DistanceMeter { get; set; }

    [Range(1, 12)]
    public int MaxLanes { get; set; }
}
