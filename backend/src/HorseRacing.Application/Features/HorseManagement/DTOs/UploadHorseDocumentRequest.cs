using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.HorseManagement.DTOs;

public class UploadHorseDocumentRequest
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string DocumentUrl { get; set; } = string.Empty;
}
