using System;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class TournamentResponse
{
    public long TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Upcoming";
    public List<RoundResponse> Rounds { get; set; } = new List<RoundResponse>();
}

public class RoundResponse
{
    public long RoundId { get; set; }
    public long TournamentId { get; set; }
    public string? Name { get; set; }
    public int RoundNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Scheduled";
}
