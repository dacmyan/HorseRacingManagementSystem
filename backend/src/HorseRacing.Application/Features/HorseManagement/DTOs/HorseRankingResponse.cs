namespace HorseRacing.Application.Features.HorseManagement.DTOs;

public class HorseRankingResponse
{
    public int HorseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Breed { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int WinsCount { get; set; }
}
