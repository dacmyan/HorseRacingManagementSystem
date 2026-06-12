using System;

namespace HorseRacing.Domain.Entities;

public class WalletTransaction
{
    public int TransactionId { get; set; }
    
    public int WalletId { get; set; }
    public Wallet? Wallet { get; set; }
    
    public int? BetId { get; set; }
    public Bet? Bet { get; set; }
    
    public int? PayoutId { get; set; }
    public Payout? Payout { get; set; }
    
    public int? PrizePayoutId { get; set; }
    public TournamentPrizePayout? TournamentPrizePayout { get; set; }
    
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
