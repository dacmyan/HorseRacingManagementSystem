namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

/// <summary>
/// Represents a horse that is currently assigned to a race and can be submitted for re-examination.
/// </summary>
public class AssignedRaceEntryResponse
{
    public long RaceEntryId { get; set; }
    public long RaceId { get; set; }
    public string? RaceName { get; set; }
    public DateTime RaceDate { get; set; }
    public string RaceStatus { get; set; } = string.Empty;
    public int LaneNo { get; set; }
    public string RaceEntryStatus { get; set; } = string.Empty;

    public long RegistrationId { get; set; }
    public string? HorseName { get; set; }
    public string? OwnerName { get; set; }
    public string? JockeyName { get; set; }

    public string? TournamentName { get; set; }
    public string? LastMedicalResult { get; set; }
    public string? LastCheckType { get; set; }
    public DateTime? LastCheckedAt { get; set; }
}
