namespace HorseRacing.Application.Features.FinancialRewards.DTOs;

public class PrizePayoutRequest
{
    public int TournamentId { get; set; }
    public decimal FirstPlacePrize { get; set; }
    public decimal SecondPlacePrize { get; set; }
    public decimal ThirdPlacePrize { get; set; }
    public int? TriggeredByUserId { get; set; }
}
