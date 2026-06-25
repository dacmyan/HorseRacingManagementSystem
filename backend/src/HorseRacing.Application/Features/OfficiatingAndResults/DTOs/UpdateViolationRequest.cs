namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class UpdateViolationRequest
{
    public string Penalty { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
