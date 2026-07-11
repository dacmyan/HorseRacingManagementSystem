namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

/// <summary>
/// DTO for performing a re-examination on a horse already assigned to a race.
/// If MedicalResult = "Fail", the system will automatically:
/// - Create a new MedicalCheckRecord with CheckType = "ReCheck"
/// - Update Registration.Status to "Disqualified"
/// - Update RaceEntry.Status to "Withdrawn" (if race not started) or "DNF" (if in progress)
/// - Send notifications to Owner, Jockey, Referees, and Bettors
/// </summary>
public class RecheckMedicalRequest
{
    public long RegistrationId { get; set; }
    public decimal Weight { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }

    /// <summary>"Negative" or "Positive"</summary>
    public string DopingResult { get; set; } = "Negative";

    /// <summary>"Pass" or "Fail"</summary>
    public string MedicalResult { get; set; } = "Pass";

    /// <summary>
    /// Required when MedicalResult is "Fail".
    /// E.g. "FailedMedicalReCheck", "VeterinaryDecision", "HorseInjury"
    /// </summary>
    public string? FailReason { get; set; }

    public string? Notes { get; set; }
}
