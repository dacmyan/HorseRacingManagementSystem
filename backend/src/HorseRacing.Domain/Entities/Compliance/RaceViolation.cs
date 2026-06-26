using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RaceViolation
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Penalty { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
