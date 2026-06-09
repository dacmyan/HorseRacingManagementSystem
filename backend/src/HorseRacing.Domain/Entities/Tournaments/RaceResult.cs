namespace HorseRacing.Domain.Entities;

public class RaceResult
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public string Winner { get; set; } = string.Empty;
}
