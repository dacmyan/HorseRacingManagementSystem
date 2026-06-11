using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class CreateRegistrationRequest
{
    [Required]
    public int TournamentId { get; set; }

    [Required]
    public int HorseId { get; set; }
}
