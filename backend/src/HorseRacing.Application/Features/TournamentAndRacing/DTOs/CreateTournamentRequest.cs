using System;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateTournamentRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfRounds { get; set; } = 2;
}
