namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class CreateMedicalCheckRequest
{
    public long RegistrationId { get; set; }
    public decimal Weight { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }

    /// <summary>"Negative" or "Positive"</summary>
    public string DopingResult { get; set; } = "Negative";

    /// <summary>"Pass" or "Fail"</summary>
    public string MedicalResult { get; set; } = "Pass";

    public string? Notes { get; set; }
}
