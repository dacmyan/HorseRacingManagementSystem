using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class Bet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public long HorseId { get; set; }
    public Horse? Horse { get; set; }
    public decimal Amount { get; set; }
    public decimal Odds { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Won, Lost, PaidOut
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
