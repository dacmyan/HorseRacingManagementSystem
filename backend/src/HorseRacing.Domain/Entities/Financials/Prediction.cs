using System;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Domain.Entities;

public class Prediction
{
    public int PredictionId { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public long RaceId { get; set; }
    public Race? Race { get; set; }
    public long RaceEntryId { get; set; }
    public RaceEntry? RaceEntry { get; set; }
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Evaluated
    public bool? IsCorrect { get; set; }
    public int Point { get; set; } = 0;
}
