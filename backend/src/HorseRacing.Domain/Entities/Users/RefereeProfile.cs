namespace HorseRacing.Domain.Entities;

public class RefereeProfile
{
    public long RefereeId { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Status { get; set; } = "Active";
}