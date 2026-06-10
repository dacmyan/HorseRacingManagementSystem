namespace HorseRacing.Application.Features.UserManagement.DTOs;

public class JockeyRankingResponse
{
    public int JockeyId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public int RankingPoint { get; set; }
}
