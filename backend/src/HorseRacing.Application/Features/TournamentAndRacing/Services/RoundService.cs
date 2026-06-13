using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class RoundService : IRoundService
{
    private readonly IRoundRepository _roundRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public RoundService(IRoundRepository roundRepository, ITournamentRepository tournamentRepository)
    {
        _roundRepository = roundRepository;
        _tournamentRepository = tournamentRepository;
    }

    public async Task<List<RoundDetailResponse>?> GetRoundsByTournamentIdAsync(long tournamentId)
    {
        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
        {
            return null;
        }

        var rounds = await _roundRepository.GetRoundsByTournamentIdAsync(tournamentId);

        return rounds.Select(r => new RoundDetailResponse
        {
            RoundId = r.RoundId,
            TournamentId = r.TournamentId,
            TournamentName = r.Tournament?.Name ?? string.Empty,
            Name = r.Name ?? string.Empty,
            RoundNumber = r.RoundNumber,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Status = r.Status,
            Races = r.Races
                .OrderBy(race => race.RaceDate)
                .Select(race => new RoundRaceResponse
                {
                    RaceId = race.RaceId,
                    Name = race.Name ?? string.Empty,
                    RaceDate = race.RaceDate,
                    DistanceMeter = race.DistanceMeter,
                    MaxLanes = race.MaxLanes,
                    Status = race.Status
                }).ToList()
        }).ToList();
    }

    public async Task<RoundDetailResponse?> GetRoundByIdAsync(long roundId)
    {
        var round = await _roundRepository.GetRoundWithDetailsAsync(roundId);
        if (round == null)
        {
            return null;
        }

        return new RoundDetailResponse
        {
            RoundId = round.RoundId,
            TournamentId = round.TournamentId,
            TournamentName = round.Tournament?.Name ?? string.Empty,
            Name = round.Name ?? string.Empty,
            RoundNumber = round.RoundNumber,
            StartDate = round.StartDate,
            EndDate = round.EndDate,
            Status = round.Status,
            Races = round.Races
                .OrderBy(race => race.RaceDate)
                .Select(race => new RoundRaceResponse
                {
                    RaceId = race.RaceId,
                    Name = race.Name ?? string.Empty,
                    RaceDate = race.RaceDate,
                    DistanceMeter = race.DistanceMeter,
                    MaxLanes = race.MaxLanes,
                    Status = race.Status
                }).ToList()
        };
    }
}
