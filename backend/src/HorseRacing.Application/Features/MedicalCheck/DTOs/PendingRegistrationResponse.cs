using System;

namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class PendingRegistrationResponse
{
    public long RegistrationId { get; set; }
    public string HorseName { get; set; } = string.Empty;
    public string TournamentName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
