using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class RespondToContractRequest
{
    [Required]
    [RegularExpression("^(Active|Accepted|Rejected)$", ErrorMessage = "Status must be either 'Accepted' or 'Rejected'.")]
    public string Status { get; set; } = string.Empty;
}
