using System;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.HorseManagement.DTOs;

public class HorseDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public List<HorseDocumentResponse> Documents { get; set; } = new();
    public HorseStatisticResponse? Statistic { get; set; }
}

public class HorseDocumentResponse
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
