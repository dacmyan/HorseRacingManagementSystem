namespace HorseRacing.Domain.Entities;

public class Horse
{
    public long HorseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public string HealthStatus { get; set; } = "Healthy";

    public int OwnerId { get; set; }

    public AppUser? Owner { get; set; }

    public ICollection<Registration> Registrations { get; set; }
        = new List<Registration>();

    public ICollection<RaceEntry> RaceEntries { get; set; }
        = new List<RaceEntry>();

    public ICollection<HorseDocument> Documents { get; set; }
        = new List<HorseDocument>();

    public HorseStatistic? Statistic { get; set; }
}
