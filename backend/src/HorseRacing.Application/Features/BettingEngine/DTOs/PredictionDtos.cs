using System;

namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class CreatePredictionRequest
{
    public long RaceId { get; set; }
    public long RaceEntryId { get; set; }
}

public class PredictionResponse
{
    public int PredictionId { get; set; }
    public long RaceId { get; set; }
    public long RaceEntryId { get; set; }
    public string Status { get; set; } = "Pending";
    public bool? IsCorrect { get; set; }
    public int Point { get; set; }
    public DateTime PredictedAt { get; set; }
}
