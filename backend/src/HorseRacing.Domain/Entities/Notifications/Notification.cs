using System;

namespace HorseRacing.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Tournament, Race, Bet, Wallet, System
    public int? ReferenceId { get; set; }
    public string? Thumbnail { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // For compatibility with legacy code
    public string Message
    {
        get => Content;
        set => Content = value;
    }
}
