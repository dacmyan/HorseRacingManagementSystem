namespace HorseRacing.Domain.Entities;

public class MedicalCheckRecord
{
    public long Id { get; set; }

    public long? RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public long? HorseId { get; set; }
    public Horse? Horse { get; set; }

    /// <summary>UserId of the staff/referee/admin who performed the check.</summary>
    public int UserId { get; set; }
    public AppUser? Veterinarian { get; set; }

    /// <summary>"Initial" or "ReCheck"</summary>
    public string CheckType { get; set; } = "Initial";

    public decimal Weight { get; set; }

    public decimal? Temperature { get; set; }

    public int? HeartRate { get; set; }

    /// <summary>E.g. "Negative", "Positive"</summary>
    public string DopingResult { get; set; } = "Negative";

    /// <summary>E.g. "Pass", "Fail"</summary>
    public string MedicalResult { get; set; } = "Pass";

    /// <summary>Reason for failure, required when MedicalResult = "Fail"</summary>
    public string? FailReason { get; set; }

    public string? Notes { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

