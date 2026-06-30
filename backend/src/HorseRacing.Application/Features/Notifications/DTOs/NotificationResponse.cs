using System;

namespace HorseRacing.Application.Features.Notifications.DTOs;

public class NotificationResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty; // Keep for compatibility
    public string Type { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public string? Thumbnail { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
}
