namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class UpdateMedicalCheckRequest
{
    public decimal? Weight { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string? DopingResult { get; set; }
    public string? MedicalResult { get; set; }
    public string? Notes { get; set; }
}
