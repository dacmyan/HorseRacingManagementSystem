using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.ContractAndRegistration.DTOs;

public class ReviewRegistrationRequest
{
    [Required]
    [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status must be either 'Approved' or 'Rejected'.")]
    public string Status { get; set; } = string.Empty;
}
