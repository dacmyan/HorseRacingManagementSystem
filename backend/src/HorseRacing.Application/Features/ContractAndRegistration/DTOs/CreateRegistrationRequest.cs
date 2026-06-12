using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class CreateRegistrationRequest
{
    [Required]
    public long TournamentId { get; set; }

    [Required]
    public int HorseId { get; set; }
}
