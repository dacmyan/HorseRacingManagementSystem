namespace HorseRacing.Domain.Entities;

public class Prediction
{
    public int Id { get; set; }
    public long RaceId { get; set; }
    public int UserId { get; set; }
    public string PredictedWinner { get; set; } = string.Empty;
}
