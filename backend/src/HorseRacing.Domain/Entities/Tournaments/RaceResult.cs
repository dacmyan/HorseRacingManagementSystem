namespace HorseRacing.Domain.Entities;

public class RaceResult
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public string Winner { get; set; } = string.Empty;
    public DateTime ResultRecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

