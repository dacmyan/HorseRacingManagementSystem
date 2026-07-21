using System;

namespace HorseRacing.Application.Features.MedicalCheck.DTOs;

public class UnhealthyHorseResponse
{
    public long HorseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}
