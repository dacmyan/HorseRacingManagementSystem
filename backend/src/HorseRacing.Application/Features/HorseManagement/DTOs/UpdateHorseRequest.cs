using System;
using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.HorseManagement.DTOs;

public class UpdateHorseRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime Age { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public string Breed { get; set; } = string.Empty;

    [Required]
    public string HealthStatus { get; set; } = "Healthy";
}
