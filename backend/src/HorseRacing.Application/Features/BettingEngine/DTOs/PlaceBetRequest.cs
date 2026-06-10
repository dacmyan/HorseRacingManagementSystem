namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class PlaceBetRequest
{
    public int RaceId { get; set; }
    public int HorseId { get; set; }
    public decimal Amount { get; set; }
}
