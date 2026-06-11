using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class Prize
{
    public int Id { get; set; }
    public long TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public int Rank { get; set; } // 1, 2, 3
    public decimal Amount { get; set; }
    public decimal OwnerPercentage { get; set; } // e.g. 70 (%)
    public decimal JockeyPercentage { get; set; } // e.g. 30 (%)
}
