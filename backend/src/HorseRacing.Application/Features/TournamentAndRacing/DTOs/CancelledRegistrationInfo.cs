namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

/// <summary>
/// Information about a registration that was auto-cancelled due to missing jockey.
/// </summary>
public class CancelledRegistrationInfo
{
    public long RegistrationId { get; set; }
    public int OwnerId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public string TournamentName { get; set; } = string.Empty;
    public long TournamentId { get; set; }
}
