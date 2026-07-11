namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

/// <summary>
/// Response returned after a re-examination that resulted in horse withdrawal.
/// </summary>
public class RecheckResultResponse
{
    public MedicalCheckResponse MedicalRecord { get; set; } = null!;
    public bool HorseWithdrawn { get; set; }
    public string? WithdrawStatus { get; set; }  // "Withdrawn", "DNF", "Scratch", or null if Pass
    public string? WithdrawReason { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
