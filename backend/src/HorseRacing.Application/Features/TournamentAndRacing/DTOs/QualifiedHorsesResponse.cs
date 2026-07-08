namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class QualifiedHorsesResponse
{
    public int TotalRegistration { get; set; }
    public int ApprovedRegistration { get; set; }
    public int MedicalPassed { get; set; }
    public int QualifiedHorses { get; set; }
    public bool CanAutoArrange { get; set; }
    public string? ValidationMessage { get; set; }
}
