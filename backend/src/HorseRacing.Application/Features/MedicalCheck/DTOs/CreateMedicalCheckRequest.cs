namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class CreateMedicalCheckRequest
{
    public long? RegistrationId { get; set; }
    public long? MedicalRecordId { get; set; }
    public long? HorseId { get; set; }

    /// <summary>"Initial" or "ReCheck"</summary>
    public string CheckType { get; set; } = "Initial";

    public decimal Weight { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }

    /// <summary>"Negative" or "Positive"</summary>
    public string DopingResult { get; set; } = "Negative";

    /// <summary>"Pass" or "Fail"</summary>
    public string MedicalResult { get; set; } = "Pass";

    /// <summary>Required when MedicalResult is "Fail"</summary>
    public string? FailReason { get; set; }

    public string? Notes { get; set; }
}
