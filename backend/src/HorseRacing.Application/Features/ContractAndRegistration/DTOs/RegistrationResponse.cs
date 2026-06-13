using System;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class RegistrationResponse
{
    public long RegistrationId { get; set; }
    public long TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public long HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
