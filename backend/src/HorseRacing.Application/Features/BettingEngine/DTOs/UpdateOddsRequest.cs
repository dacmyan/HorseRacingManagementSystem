namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class UpdateOddsRequest
{
    public int RaceId { get; set; }
    public int HorseId { get; set; }
    public decimal Odds { get; set; }
}
