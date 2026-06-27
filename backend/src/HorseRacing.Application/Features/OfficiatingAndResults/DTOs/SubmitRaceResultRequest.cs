using System.Collections.Generic;

namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class RaceEntryResultInput
{
    public long RaceEntryId { get; set; }
    public int FinishPosition { get; set; }
    public decimal FinishTime { get; set; }
}

public class SubmitRaceResultRequest
{
    public long RaceId { get; set; }
    public string Winner { get; set; } = string.Empty;
    public int? RefereeId { get; set; }
    public List<RaceEntryResultInput>? Entries { get; set; }
}
