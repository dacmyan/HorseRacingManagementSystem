namespace HorseRacing.Domain.Entities;

public class MedicalCheckRecord
{
    public long Id { get; set; }

    public long RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    /// <summary>UserId of the staff/referee/admin who performed the check.</summary>
    public int UserId { get; set; }
    public AppUser? Veterinarian { get; set; }

    public decimal Weight { get; set; }

    public decimal? Temperature { get; set; }

    public int? HeartRate { get; set; }

    /// <summary>E.g. "Negative", "Positive"</summary>
    public string DopingResult { get; set; } = "Negative";

    /// <summary>E.g. "Pass", "Fail"</summary>
    public string MedicalResult { get; set; } = "Pass";

    public string? Notes { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
