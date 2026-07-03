using System;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateTournamentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfRounds { get; set; } = 2;
    public List<PrizeConfigRequest>? Prizes { get; set; }
}

public class PrizeConfigRequest
{
    public int RankPosition { get; set; }
    public decimal Amount { get; set; }
    public decimal OwnerPercentage { get; set; }
    public decimal JockeyPercentage { get; set; }
}
