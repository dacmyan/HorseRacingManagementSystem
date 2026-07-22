namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CloseRegistrationResponse
{
    public long TournamentId { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QualifiedHorses { get; set; }
    public int CancelledRegistrations { get; set; }
    public int CancelledPendingRegistrations { get; set; }
    public int MinimumRequired { get; set; } = 12;
    public int MaximumAllowed { get; set; } = 48;
    public bool CanGenerateRaces { get; set; }
}
