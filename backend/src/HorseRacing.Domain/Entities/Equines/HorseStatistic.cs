using System;

namespace HorseRacing.Domain.Entities;

public class HorseStatistic
{
    public int Id { get; set; }
    public long HorseId { get; set; }
    public Horse? Horse { get; set; }
    public int TotalRaces { get; set; }
    public int TotalWins { get; set; }
    public int TotalSecondPlaces { get; set; }
    public int TotalThirdPlaces { get; set; }
    public decimal AverageSpeed { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
