using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class RespondToContractRequest
{
    [Required]
    [RegularExpression("^(Active|Rejected)$", ErrorMessage = "Status must be either 'Active' or 'Rejected'.")]
    public string Status { get; set; } = string.Empty;
}
