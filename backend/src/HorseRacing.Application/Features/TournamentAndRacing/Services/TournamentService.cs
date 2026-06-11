using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository;

    public TournamentService(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    public async Task<TournamentResponse> CreateTournamentAsync(CreateTournamentRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Tournament name cannot be empty.", nameof(request.Name));
        }

        if (request.NumberOfRounds <= 0)
        {
            throw new ArgumentException("Number of rounds must be greater than zero.", nameof(request.NumberOfRounds));
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(request.EndDate));
        }

        var tournament = new Tournament
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Upcoming"
        };

        for (int i = 1; i <= request.NumberOfRounds; i++)
        {
            tournament.Rounds.Add(new Round
            {
                Name = $"Round {i}",
                RoundNumber = i,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "Scheduled"
            });
        }

        await _tournamentRepository.AddAsync(tournament);
        await _tournamentRepository.SaveChangesAsync();

        return new TournamentResponse
        {
            TournamentId = tournament.TournamentId,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Status = tournament.Status,
            Rounds = tournament.Rounds.Select(r => new RoundResponse
            {
                RoundId = r.RoundId,
                TournamentId = r.TournamentId,
                Name = r.Name,
                RoundNumber = r.RoundNumber,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status
            }).ToList()
        };
    }
}
