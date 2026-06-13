namespace HorseRacing.Domain.Entities;

public class JockeyProfile
{
    public long JockeyId { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int ExperienceYears { get; set; }
    public int RankingPoint { get; set; }
    public string Status { get; set; } = "Active";
}