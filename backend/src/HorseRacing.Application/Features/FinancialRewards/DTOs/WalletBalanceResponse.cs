namespace HorseRacing.Application.Features.FinancialRewards.DTOs;

public class WalletBalanceResponse
{
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
}
