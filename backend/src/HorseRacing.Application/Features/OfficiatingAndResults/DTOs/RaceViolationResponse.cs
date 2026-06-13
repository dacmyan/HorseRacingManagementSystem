using System;

namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class RaceViolationResponse
{
    public long Id { get; set; }
    public long RaceId { get; set; }
    public long RaceEntryId { get; set; }
    public long RefereeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Penalty { get; set; }
    public DateTime CreatedAt { get; set; }
}
