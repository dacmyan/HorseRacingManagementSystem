namespace HorseRacing.Domain.Entities;

using HorseRacing.Domain.Entities.Tournaments;

public class Registration
{
    public long RegistrationId { get; set; }

    public long TournamentId { get; set; }

    public Tournament? Tournament { get; set; }

    public long HorseId { get; set; }

    public Horse? Horse { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}