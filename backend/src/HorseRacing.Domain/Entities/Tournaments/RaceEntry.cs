using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class RaceEntry
{
    public long RaceEntryId { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public long RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public int? JockeyId { get; set; }
    public JockeyProfile? JockeyProfile { get; set; }
    public decimal? WinningProbability { get; set; }
    public decimal? CurrentOdds { get; set; }
    public int LaneNo { get; set; }

    /// <summary>Pending | Confirmed | Withdrawn | Scratch | Finished | DNF | Disqualified</summary>
    public string Status { get; set; } = "Ready";

    public decimal? FinishTime { get; set; }
    public int? FinishPosition { get; set; }

    /// <summary>E.g. "FailedMedicalReCheck", "VeterinaryDecision", "HorseInjury", "Scratch"</summary>
    public string? WithdrawReason { get; set; }

    public DateTime? WithdrawTime { get; set; }
}

