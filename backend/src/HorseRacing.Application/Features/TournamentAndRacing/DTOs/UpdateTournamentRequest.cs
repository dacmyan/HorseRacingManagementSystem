using System;
using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class UpdateTournamentRequest
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
    [StringLength(30)]
    public string Status { get; set; } = string.Empty;
}
