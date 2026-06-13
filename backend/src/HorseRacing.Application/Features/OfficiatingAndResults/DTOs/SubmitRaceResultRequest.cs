namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class SubmitRaceResultRequest
{
    public long RaceId { get; set; }
    public string Winner { get; set; } = string.Empty;
    public int? RefereeId { get; set; }
}
