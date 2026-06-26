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

        if (request.StartDate < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new ArgumentException("Start date cannot be in the past.", nameof(request.StartDate));
        }

        var now = DateTime.UtcNow;
        var status = "Upcoming";
        if (request.StartDate <= now && now <= request.EndDate)
        {
            status = "Active";
        }
        else if (now > request.EndDate)
        {
            status = "Completed";
        }

        var tournament = new Tournament
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = status
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

        if (request.StartDate < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new ArgumentException("Start date cannot be in the past.", nameof(request.StartDate));
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

        var now = DateTime.UtcNow;
        var status = "Upcoming";
        if (request.StartDate <= now && now <= request.EndDate)
        {
            status = "Active";
        }
        else if (now > request.EndDate)
        {
            status = "Completed";
        }
        tournament.Status = status;

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

    public async Task<List<RaceScheduleResponse>> GenerateRacesForTournamentAsync(long tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(tournamentId);
        if (tournament == null) throw new KeyNotFoundException($"Tournament {tournamentId} not found.");

        var registrations = await _tournamentRepository.GetApprovedRegistrationsAsync(tournamentId);
        if (!registrations.Any()) throw new InvalidOperationException("No approved registrations found for this tournament.");

        var firstRound = tournament.Rounds.OrderBy(r => r.RoundNumber).FirstOrDefault();
        if (firstRound == null) throw new InvalidOperationException("Tournament has no rounds.");

        var maxHorsePerRace = 12;
        var horseGroups = registrations
            .Select((reg, index) => new { reg, index })
            .GroupBy(x => x.index / maxHorsePerRace)
            .Select(g => g.Select(x => x.reg).ToList())
            .ToList();

        var newRaces = new List<HorseRacing.Domain.Entities.Tournaments.Race>();
        var newEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();

        int raceCounter = 1;
        foreach (var group in horseGroups)
        {
            var race = new HorseRacing.Domain.Entities.Tournaments.Race
            {
                RoundId = firstRound.RoundId,
                Name = $"Race {raceCounter}",
                DistanceMeter = 1200,
                MaxLanes = maxHorsePerRace,
                Status = "Scheduled",
                RaceDate = tournament.StartDate ?? DateTime.UtcNow.AddDays(1)
            };
            newRaces.Add(race);
            
            int lane = 1;
            foreach (var reg in group)
            {
                var entry = new HorseRacing.Domain.Entities.RaceEntry
                {
                    Race = race,
                    RegistrationId = reg.RegistrationId,
                    LaneNo = lane++,
                    Status = "Confirmed",
                    WinningProbability = 0.5m,
                    CurrentOdds = 2.0m
                };
                newEntries.Add(entry);
            }
            raceCounter++;
        }

        await _tournamentRepository.AddRacesAsync(newRaces);
        await _tournamentRepository.AddRaceEntriesAsync(newEntries);
        await _tournamentRepository.SaveChangesAsync();

        return newRaces.Select(r => new RaceScheduleResponse
        {
            RaceId = r.RaceId,
            RoundId = r.RoundId,
            Name = r.Name ?? string.Empty,
            RaceDate = r.RaceDate,
            DistanceMeter = r.DistanceMeter,
            MaxLanes = r.MaxLanes,
            Status = r.Status ?? string.Empty
        }).ToList();
    }

    private static TournamentResponse MapToResponse(Tournament tournament)
    {
        var now = DateTime.UtcNow;
        var calculatedStatus = tournament.Status;

        if (tournament.StartDate.HasValue && tournament.EndDate.HasValue)
        {
            if (now < tournament.StartDate.Value)
            {
                calculatedStatus = "Upcoming";
            }
            else if (now >= tournament.StartDate.Value && now <= tournament.EndDate.Value)
            {
                calculatedStatus = "Active";
            }
            else if (now > tournament.EndDate.Value)
            {
                calculatedStatus = "Completed";
            }
        }

        return new TournamentResponse
        {
            TournamentId = tournament.TournamentId,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Status = calculatedStatus,
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
