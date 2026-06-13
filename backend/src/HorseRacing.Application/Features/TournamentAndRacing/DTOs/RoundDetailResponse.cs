using System;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class RoundDetailResponse
{
    public long RoundId { get; set; }
    public long TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Scheduled";
    public List<RoundRaceResponse> Races { get; set; } = new List<RoundRaceResponse>();
}
