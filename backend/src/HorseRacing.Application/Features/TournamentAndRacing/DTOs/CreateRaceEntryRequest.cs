namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateRaceEntryRequest
{
    public long RegistrationId { get; set; }
    public int? JockeyId { get; set; }
    public int LaneNo { get; set; }
    public decimal? WinningProbability { get; set; }
    public decimal? CurrentOdds { get; set; }
}
