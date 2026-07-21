using System.ComponentModel.DataAnnotations;

namespace HorseRacing.Application.Features.TournamentAndRacing.DTOs;

public class CreateRaceEntryRequest
{
    [Range(1, long.MaxValue)]
    public long RegistrationId { get; set; }
    [Range(1, int.MaxValue)]
    public int? JockeyId { get; set; }
    [Range(1, 12)]
    public int LaneNo { get; set; }
    [Range(typeof(decimal), "0", "100")]
    public decimal? WinningProbability { get; set; }
    [Range(typeof(decimal), "1.01", "1000000")]
    public decimal? CurrentOdds { get; set; }
}
