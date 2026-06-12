using System;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class RegistrationResponse
{
    public int Id { get; set; }
    public long TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public int HorseId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
