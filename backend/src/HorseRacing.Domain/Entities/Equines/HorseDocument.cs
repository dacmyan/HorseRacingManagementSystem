using System;

namespace HorseRacing.Domain.Entities;

public class HorseDocument
{
    public int Id { get; set; }
    public int HorseId { get; set; }
    public Horse? Horse { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
