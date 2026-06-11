namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class PlaceBetRequest
{
    public long RaceId { get; set; }
    public int HorseId { get; set; }
    public decimal Amount { get; set; }
}
