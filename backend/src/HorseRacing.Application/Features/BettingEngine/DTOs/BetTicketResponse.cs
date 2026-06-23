using System;

namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class BetTicketResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public long RaceId { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public int HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Odds { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal PotentialPayout { get; set; }
    public decimal? ActualPayout { get; set; }
    public string? PayoutStatus { get; set; }
}
