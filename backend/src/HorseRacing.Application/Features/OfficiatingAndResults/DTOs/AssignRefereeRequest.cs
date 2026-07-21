using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.OfficiatingAndResults.DTOs;

public class AssignRefereeRequest
{
    [Range(1, int.MaxValue)]
    public int RefereeId { get; set; }
}
