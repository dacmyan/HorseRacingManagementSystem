namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class PredictionManagementRequest
{
    public int RaceId { get; set; }
    public string PredictedWinner { get; set; } = string.Empty;
}
