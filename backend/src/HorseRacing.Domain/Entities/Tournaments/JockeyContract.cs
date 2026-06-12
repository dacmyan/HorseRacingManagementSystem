namespace HorseRacing.Domain.Entities;

public class JockeyContract
{
    public int Id { get; set; }

    public int HorseId { get; set; }

    public Horse? Horse { get; set; }

    public int OwnerId { get; set; }

    public AppUser? Owner { get; set; }

    public int JockeyId { get; set; }

    public AppUser? Jockey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}