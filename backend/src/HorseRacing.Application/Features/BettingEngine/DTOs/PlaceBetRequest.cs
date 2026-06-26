namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class PlaceBetRequest
{
    public long RaceEntryId { get; set; }
    public decimal Amount { get; set; }
}
