using System;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class UpdateTournamentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfRounds { get; set; } = 2;
    public string Status { get; set; } = string.Empty;
}
