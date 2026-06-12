using System;

namespace HorseRacing.Application.Features.HorseManagement.DTOs;

public class HorseStatisticResponse
{
    public int Id { get; set; }
    public int HorseId { get; set; }
    public int TotalRaces { get; set; }
    public int TotalWins { get; set; }
    public int TotalSecondPlaces { get; set; }
    public int TotalThirdPlaces { get; set; }
    public decimal AverageSpeed { get; set; }
    public DateTime UpdatedAt { get; set; }
}
