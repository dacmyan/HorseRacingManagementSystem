using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateTournamentRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [Range(1, 10)]
    public int NumberOfRounds { get; set; } = 2;
    public List<PrizeConfigRequest>? Prizes { get; set; }
}

public class PrizeConfigRequest
{
    [Range(1, 3)]
    public int RankPosition { get; set; }
    [Range(typeof(decimal), "0.01", "1000000000000")]
    public decimal Amount { get; set; }
    [Range(typeof(decimal), "0", "100")]
    public decimal OwnerPercentage { get; set; }
    [Range(typeof(decimal), "0", "100")]
    public decimal JockeyPercentage { get; set; }
}
