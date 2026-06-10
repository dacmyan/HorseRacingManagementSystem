using System;

namespace HorseRacing.Domain.Entities;

public class Payout
{
    public int Id { get; set; }
    public int BetId { get; set; }
    public Bet? Bet { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
