using System;
using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class CreateJockeyContract
{
    [Required]
    public int HorseId { get; set; }

    [Required]
    public int JockeyId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
