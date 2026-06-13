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

        return MapToResponse(tournament);
    }

    public async Task<TournamentResponse?> UpdateTournamentAsync(long id, UpdateTournamentRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(id);
        if (tournament == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Tournament name cannot be empty.", nameof(request.Name));
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(request.EndDate));
        }

        int currentRoundsCount = tournament.Rounds.Count;
        if (request.NumberOfRounds < currentRoundsCount)
        {
            throw new ArgumentException("Cannot decrease the number of rounds because deleting existing rounds is not supported.", nameof(request.NumberOfRounds));
        }

        // Apply basic tournament details updates
        tournament.Name = request.Name;
        tournament.StartDate = request.StartDate;
        tournament.EndDate = request.EndDate;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            tournament.Status = request.Status;
        }

        // Add new rounds if NumberOfRounds has increased
        if (request.NumberOfRounds > currentRoundsCount)
        {
            for (int i = currentRoundsCount + 1; i <= request.NumberOfRounds; i++)
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
        }

        _tournamentRepository.Update(tournament);
        await _tournamentRepository.SaveChangesAsync();

        return MapToResponse(tournament);
    }

    public async Task<List<TournamentResponse>> GetAllTournamentsAsync()
    {
        var tournaments = await _tournamentRepository.GetAllAsync();
        return tournaments.Select(MapToResponse).ToList();
    }

    public async Task<TournamentResponse?> GetTournamentByIdAsync(long id)
    {
        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(id);
        if (tournament == null)
        {
            return null;
        }

        return MapToResponse(tournament);
    }

    private static TournamentResponse MapToResponse(Tournament tournament)
    {
        return new TournamentResponse
        {
            TournamentId = tournament.TournamentId,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Status = tournament.Status,
            Rounds = tournament.Rounds
                .OrderBy(r => r.RoundNumber)
                .Select(r => new RoundResponse
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
