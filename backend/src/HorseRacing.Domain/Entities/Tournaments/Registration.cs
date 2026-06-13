namespace HorseRacing.Domain.Entities;

using HorseRacing.Domain.Entities.Tournaments;

public class Registration
{
    public int Id { get; set; }

    public long TournamentId { get; set; }

    public Tournament? Tournament { get; set; }

    public int HorseId { get; set; }

    public Horse? Horse { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}