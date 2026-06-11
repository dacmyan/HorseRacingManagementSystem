using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class TournamentPrizePayout
{
    public int Id { get; set; }
    public long TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public decimal Amount { get; set; }
    public string Role { get; set; } = string.Empty; // HorseOwner or Jockey
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
