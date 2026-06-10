using System.Collections.Generic;

namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class PredictionStatsResponse
{
    public int RaceId { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public int TotalPredictions { get; set; }
    public List<HorsePredictionStat> Details { get; set; } = new();
}

public class HorsePredictionStat
{
    public string PredictedWinner { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
