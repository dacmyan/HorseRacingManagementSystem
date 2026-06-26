using System;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.BettingEngine.DTOs;

public class RaceBettingInfoResponse
{
    public long RaceId { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public string RaceStatus { get; set; } = string.Empty;
    public string TournamentStatus { get; set; } = string.Empty;
    public bool CanBet { get; set; }
    public List<RaceEntryBettingDto> Entries { get; set; } = new();
}

public class RaceEntryBettingDto
{
    public long RaceEntryId { get; set; }
    public int LaneNo { get; set; }
    public long HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public string JockeyName { get; set; } = string.Empty;
    public decimal AverageTime { get; set; }
    public decimal RecentAverageTime { get; set; }
    public decimal WinRate { get; set; }
    public decimal WinningProbability { get; set; }
    public decimal CurrentOdds { get; set; }
}
