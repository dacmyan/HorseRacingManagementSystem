namespace HorseRacing.Domain.Entities;

public class Horse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Breed { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public AppUser? Owner { get; set; }
}
