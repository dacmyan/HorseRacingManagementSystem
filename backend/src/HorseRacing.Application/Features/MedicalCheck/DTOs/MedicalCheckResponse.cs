namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class MedicalCheckResponse
{
    public long Id { get; set; }

    public long? RegistrationId { get; set; }
    public long? HorseId { get; set; }
    public string? HorseName { get; set; }
    public string? TournamentName { get; set; }

    public int UserId { get; set; }
    public string? CheckedByName { get; set; }

    public string CheckType { get; set; } = "Initial";

    public decimal Weight { get; set; }
    public decimal? Temperature { get; set; }
    public int? HeartRate { get; set; }
    public string DopingResult { get; set; } = string.Empty;
    public string MedicalResult { get; set; } = string.Empty;
    public string? FailReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CheckedAt { get; set; }
}
