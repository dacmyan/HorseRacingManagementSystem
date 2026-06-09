namespace HorseRacing.Domain.Entities;

public class Race
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public int Distance { get; set; }
    public string Status { get; set; } = string.Empty;
}
