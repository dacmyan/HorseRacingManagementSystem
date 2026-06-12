using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities.Financials;

public class Prize
{
    public int Id { get; set; }
    public long TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public int RankPosition { get; set; }
    public decimal Amount { get; set; }
    public decimal OwnerPercentage { get; set; }
    public decimal JockeyPercentage { get; set; }
}
