using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.BettingEngine.Interfaces;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IBettingService? _bettingService;

    public TournamentService(
        ITournamentRepository tournamentRepository,
        IBettingService? bettingService = null)
    {
        _tournamentRepository = tournamentRepository;
        _bettingService = bettingService;
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

        if (request.NumberOfRounds != 0 && request.NumberOfRounds != 2)
        {
            throw new ArgumentException("Tournament must have exactly 2 rounds: Pre and Final.", nameof(request.NumberOfRounds));
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

        var preRound = new Round
        {
            Name = "Pre",
            RoundNumber = 1,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Scheduled"
        };

        var finalRound = new Round
        {
            Name = "Final",
            RoundNumber = 2,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Scheduled"
        };
        finalRound.Races.Add(new HorseRacing.Domain.Entities.Tournaments.Race
        {
            Name = "Final Race",
            RaceDate = request.EndDate,
            DistanceMeter = 1600,
            MaxLanes = 12,
            Status = "Scheduled"
        });

        tournament.Rounds.Add(preRound);
        tournament.Rounds.Add(finalRound);

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

        if (request.NumberOfRounds != 0 && request.NumberOfRounds != 2)
        {
            throw new ArgumentException("Tournament must have exactly 2 rounds: Pre and Final.", nameof(request.NumberOfRounds));
        }

        int currentRoundsCount = tournament.Rounds.Count;

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
        const int defaultRoundCount = 2;
        if (defaultRoundCount > currentRoundsCount)
        {
            var roundNames = new[] { "Pre", "Final" };
            for (int i = currentRoundsCount + 1; i <= defaultRoundCount; i++)
            {
                tournament.Rounds.Add(new Round
                {
                    Name = roundNames[i - 1],
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

        var rounds = tournament.Rounds.OrderBy(r => r.RoundNumber).ToList();
        if (!rounds.Any()) throw new InvalidOperationException("Tournament has no rounds.");
        if (rounds.Count != 2) throw new InvalidOperationException("Tournament must have exactly 2 rounds: Pre and Final.");

        var firstRound = rounds.FirstOrDefault();
        if (firstRound == null) throw new InvalidOperationException("Tournament has no rounds.");

        var secondRound = rounds[1]; // Final
        var prefinalRaces = await _tournamentRepository.GetRacesByRoundIdAsync(firstRound.RoundId);
        var registrations = await _tournamentRepository.GetApprovedRegistrationsAsync(tournamentId);
        if (!registrations.Any()) throw new InvalidOperationException("No approved registrations found for this tournament.");

        var assignedRegistrationIds = new HashSet<long>();
        foreach (var race in prefinalRaces)
        {
            var entries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(race.RaceId);
            foreach (var entry in entries)
            {
                assignedRegistrationIds.Add(entry.RegistrationId);
            }
        }

        var unassignedRegistrations = registrations.Where(r => !assignedRegistrationIds.Contains(r.RegistrationId)).ToList();

        if (unassignedRegistrations.Any())
        {
            // Case 1: Prefinal races do not exist yet or there are unassigned horses.
            var maxHorsePerRace = 12;
            var activePrefinalJockeyByHorseId = await _tournamentRepository.GetActiveJockeyProfileIdsByHorseAsync(
                tournamentId,
                unassignedRegistrations.Select(r => r.HorseId)) ?? new Dictionary<long, int>();

            var newRaces = new List<HorseRacing.Domain.Entities.Tournaments.Race>();
            var newEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();

            // 1. First, try to fill existing prefinal races that are not full
            foreach (var race in prefinalRaces)
            {
                if (race.Status != "Scheduled") continue; // only fill scheduled races

                var existingEntries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(race.RaceId);
                int currentCount = existingEntries.Count;
                int maxLanes = race.MaxLanes > 0 ? race.MaxLanes : maxHorsePerRace;

                if (currentCount < maxLanes)
                {
                    int slotsAvailable = maxLanes - currentCount;
                    var batchToFill = unassignedRegistrations.Take(slotsAvailable).ToList();
                    
                    int lane = currentCount + 1;
                    foreach (var reg in batchToFill)
                    {
                        var entry = new HorseRacing.Domain.Entities.RaceEntry
                        {
                            RaceId = race.RaceId,
                            RegistrationId = reg.RegistrationId,
                            JockeyId = GetAssignedJockeyId(activePrefinalJockeyByHorseId, reg.HorseId),
                            LaneNo = lane++,
                            Status = "Confirmed",
                            WinningProbability = 0.5m,
                            CurrentOdds = 2.0m
                        };
                        newEntries.Add(entry);
                    }
                    unassignedRegistrations = unassignedRegistrations.Skip(slotsAvailable).ToList();
                }

                if (!unassignedRegistrations.Any()) break;
            }

            // 2. If there are still unassigned registrations, create new races
            if (unassignedRegistrations.Any())
            {
                var horseGroups = BuildRaceGroups(unassignedRegistrations, maxHorsePerRace);
                int raceCounter = prefinalRaces.Count + 1;
                foreach (var group in horseGroups)
                {
                    var race = new HorseRacing.Domain.Entities.Tournaments.Race
                    {
                        RoundId = firstRound.RoundId,
                        Name = $"Race {raceCounter} (Pre)",
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
                            JockeyId = GetAssignedJockeyId(activePrefinalJockeyByHorseId, reg.HorseId),
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
            }

            await _tournamentRepository.AddRaceEntriesAsync(newEntries);
            await _tournamentRepository.SaveChangesAsync();

            // Return new and updated races
            var updatedRaceIds = newEntries.Where(e => e.RaceId > 0).Select(e => e.RaceId).ToHashSet();
            var resultRaces = new List<RaceScheduleResponse>();
            
            foreach (var r in newRaces)
            {
                resultRaces.Add(new RaceScheduleResponse
                {
                    RaceId = r.RaceId,
                    RoundId = r.RoundId,
                    Name = r.Name ?? string.Empty,
                    RaceDate = r.RaceDate,
                    DistanceMeter = r.DistanceMeter,
                    MaxLanes = r.MaxLanes,
                    Status = r.Status ?? string.Empty
                });
            }

            foreach (var r in prefinalRaces)
            {
                if (updatedRaceIds.Contains(r.RaceId))
                {
                    resultRaces.Add(new RaceScheduleResponse
                    {
                        RaceId = r.RaceId,
                        RoundId = r.RoundId,
                        Name = r.Name ?? string.Empty,
                        RaceDate = r.RaceDate,
                        DistanceMeter = r.DistanceMeter,
                        MaxLanes = r.MaxLanes,
                        Status = r.Status ?? string.Empty
                    });
                }
            }

            return resultRaces;
        }
        else
        {
            // Case 2: Prefinal races exist. Fill the existing Final race.
            if (rounds.Count < 2)
            {
                throw new InvalidOperationException("This tournament has only 1 round configured. Cannot generate final round.");
            }

            var finalRace = await GetSingleFinalRaceAsync(secondRound.RoundId);
            var existingFinalEntries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(finalRace.RaceId);
            if (existingFinalEntries.Any())
            {
                throw new InvalidOperationException("Final race already has participants.");
            }

            // Get top 12 horses from prefinal
            var topRegistrations = await _tournamentRepository.GetTopHorsesFromPrefinalAsync(tournamentId, firstRound.RoundId);
            if (!topRegistrations.Any())
            {
                throw new InvalidOperationException("No eligible horses found to generate final round, or pre-final races are not finished yet.");
            }

            var activeFinalJockeyByHorseId = await _tournamentRepository.GetActiveJockeyProfileIdsByHorseAsync(
                tournamentId,
                topRegistrations.Select(r => r.HorseId)) ?? new Dictionary<long, int>();
            var finalEntries = CreateRaceEntriesForRegistrations(topRegistrations, finalRace.RaceId, activeFinalJockeyByHorseId);

            await _tournamentRepository.AddRaceEntriesAsync(finalEntries);
            await _tournamentRepository.SaveChangesAsync();

            return new List<RaceScheduleResponse>
            {
                new RaceScheduleResponse
                {
                    RaceId = finalRace.RaceId,
                    RoundId = finalRace.RoundId,
                    Name = finalRace.Name ?? string.Empty,
                    RaceDate = finalRace.RaceDate,
                    DistanceMeter = finalRace.DistanceMeter,
                    MaxLanes = finalRace.MaxLanes,
                    Status = finalRace.Status ?? string.Empty
                }
            };
        }
    }

    private async Task<HorseRacing.Domain.Entities.Tournaments.Race> GetSingleFinalRaceAsync(long finalRoundId)
    {
        var finalRaces = await _tournamentRepository.GetRacesByRoundIdAsync(finalRoundId);
        if (finalRaces.Count == 0)
        {
            throw new InvalidOperationException("Final round race has not been created yet.");
        }

        if (finalRaces.Count > 1)
        {
            throw new InvalidOperationException("Final round must have exactly 1 race.");
        }

        return finalRaces[0];
    }

    private static List<List<HorseRacing.Domain.Entities.Registration>> BuildRaceGroups(
        List<HorseRacing.Domain.Entities.Registration> registrations,
        int maxHorsePerRace)
    {
        var groups = registrations
            .Select((reg, index) => new { reg, index })
            .GroupBy(x => x.index / maxHorsePerRace)
            .Select(g => g.Select(x => x.reg).ToList())
            .ToList();

        var lastGroup = groups.LastOrDefault();
        if (groups.Count > 1 && lastGroup is { Count: > 0 and <= 2 })
        {
            var previousGroup = groups[^2];
            var horsesToMove = previousGroup.Count / 2;
            var movedRegistrations = previousGroup
                .Skip(previousGroup.Count - horsesToMove)
                .ToList();

            previousGroup.RemoveRange(previousGroup.Count - horsesToMove, horsesToMove);
            lastGroup.InsertRange(0, movedRegistrations);
        }

        return groups;
    }

    private static List<HorseRacing.Domain.Entities.RaceEntry> CreateRaceEntriesForRegistrations(
        IEnumerable<HorseRacing.Domain.Entities.Registration> registrations,
        long raceId,
        IReadOnlyDictionary<long, int> activeJockeyByHorseId)
    {
        var entries = new List<HorseRacing.Domain.Entities.RaceEntry>();
        int lane = 1;
        foreach (var reg in registrations)
        {
            entries.Add(new HorseRacing.Domain.Entities.RaceEntry
            {
                RaceId = raceId,
                RegistrationId = reg.RegistrationId,
                JockeyId = GetAssignedJockeyId(activeJockeyByHorseId, reg.HorseId),
                LaneNo = lane++,
                Status = "Confirmed",
                WinningProbability = 0.5m,
                CurrentOdds = 2.0m
            });
        }

        return entries;
    }

    private static int? GetAssignedJockeyId(IReadOnlyDictionary<long, int>? activeJockeyByHorseId, long horseId)
    {
        return activeJockeyByHorseId != null && activeJockeyByHorseId.TryGetValue(horseId, out var jockeyId)
            ? jockeyId
            : null;
    }

    private static RaceScheduleResponse MapRaceToScheduleResponse(HorseRacing.Domain.Entities.Tournaments.Race race)
    {
        return new RaceScheduleResponse
        {
            RaceId = race.RaceId,
            RoundId = race.RoundId,
            Name = race.Name ?? string.Empty,
            RaceDate = race.RaceDate,
            DistanceMeter = race.DistanceMeter,
            MaxLanes = race.MaxLanes,
            Status = race.Status ?? string.Empty
        };
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

    public async Task<RaceScheduleResponse> GenerateFinalRaceAsync(long tournamentId)
    {
        if (_bettingService == null)
        {
            throw new InvalidOperationException("Dependencies are not resolved.");
        }

        // 1. Verify tournament exists
        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(tournamentId);
        if (tournament == null)
        {
            throw new KeyNotFoundException($"Tournament with ID {tournamentId} was not found.");
        }

        var rounds = tournament.Rounds.OrderBy(r => r.RoundNumber).ToList();
        var preRound = rounds.FirstOrDefault(r => r.RoundNumber == 1);
        if (preRound == null)
        {
            throw new InvalidOperationException("Pre Round (Round 1) does not exist.");
        }

        var finalRound = rounds.FirstOrDefault(r => r.RoundNumber == 2);
        if (finalRound == null)
        {
            // Create final round if missing
            finalRound = new Round
            {
                TournamentId = tournamentId,
                Name = "Final",
                RoundNumber = 2,
                Status = "Scheduled",
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate
            };
            await _tournamentRepository.AddRoundAsync(finalRound);
            await _tournamentRepository.SaveChangesAsync();
        }

        // 2. Check if all Pre races are completed/finished
        var preRaces = await _tournamentRepository.GetRacesByRoundIdAsync(preRound.RoundId);

        if (preRaces.Count == 0)
        {
            throw new InvalidOperationException("Cannot generate Final Race because there are no Pre races scheduled.");
        }

        bool allPreFinished = preRaces.All(r => r.Status == "Completed" || r.Status == "Finished");
        if (!allPreFinished)
        {
            throw new InvalidOperationException("Cannot generate Final Race because not all Pre races are completed.");
        }

        // Ensure there is at least 1 race result in Pre round
        var preRaceIds = preRaces.Select(r => r.RaceId).ToList();
        var hasPreResults = await _tournamentRepository.HasRaceResultsAsync(preRaceIds);
        if (!hasPreResults)
        {
            throw new InvalidOperationException("Cannot generate Final Race because there are no race results in the Pre Round.");
        }

        // 3. Locate or create Final Race (Final Round has unique 1 Race)
        var finalRaces = await _tournamentRepository.GetRacesByRoundIdAsync(finalRound.RoundId);
        var finalRace = finalRaces.FirstOrDefault();

        if (finalRace == null)
        {
            finalRace = new HorseRacing.Domain.Entities.Tournaments.Race
            {
                RoundId = finalRound.RoundId,
                Name = "Final Race",
                RaceDate = tournament.EndDate ?? DateTime.UtcNow.AddDays(1),
                DistanceMeter = 1600,
                MaxLanes = 12,
                Status = "Scheduled"
            };
            await _tournamentRepository.AddRaceAsync(finalRace);
            await _tournamentRepository.SaveChangesAsync();
        }
        else if (finalRace.Status == "Running" || finalRace.Status == "Completed" || finalRace.Status == "Finished")
        {
            throw new InvalidOperationException("Cannot generate Final Race because it has already started or completed.");
        }

        // 4. Query Top 12 finalists based on pre-final results
        var sortedFinalists = await _tournamentRepository.GetFinalistsFromPreRoundAsync(tournamentId, preRound.RoundId);
        
        var top12Finalists = sortedFinalists
            .OrderBy(re => re.FinishTime ?? 99999m)
            .ThenBy(re => re.FinishPosition ?? 99)
            .ThenBy(re => re.Registration?.Horse?.AverageTime ?? 99999m)
            .Take(12)
            .ToList();

        if (top12Finalists.Count == 0)
        {
            throw new InvalidOperationException("No eligible finalists found from Pre Round results.");
        }

        // 5. Clear or update entries for the Final Race (no duplicates)
        var existingFinalEntries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(finalRace.RaceId);

        if (existingFinalEntries.Count > 0)
        {
            await _tournamentRepository.RemoveRaceEntriesAsync(existingFinalEntries);
            await _tournamentRepository.SaveChangesAsync();
        }

        var activeFinalJockeyByHorseId = await _tournamentRepository.GetActiveJockeyProfileIdsByHorseAsync(
            tournamentId,
            top12Finalists.Select(re => re.Registration!.HorseId)) ?? new Dictionary<long, int>();

        var newEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();
        int lane = 1;
        foreach (var finalist in top12Finalists)
        {
            var entry = new HorseRacing.Domain.Entities.RaceEntry
            {
                RaceId = finalRace.RaceId,
                RegistrationId = finalist.RegistrationId,
                JockeyId = activeFinalJockeyByHorseId.TryGetValue(finalist.Registration!.HorseId, out var jId) ? jId : finalist.JockeyId,
                LaneNo = lane++,
                Status = "Confirmed",
                WinningProbability = 0.5m,
                CurrentOdds = 2.0m
            };
            newEntries.Add(entry);
        }

        await _tournamentRepository.AddRaceEntriesAsync(newEntries);
        await _tournamentRepository.SaveChangesAsync();

        // 6. Recalculate odds for the Final Race
        await _bettingService.RecalculateRaceOddsAsync(finalRace.RaceId);

        return new RaceScheduleResponse
        {
            RaceId = finalRace.RaceId,
            RoundId = finalRace.RoundId,
            Name = finalRace.Name ?? string.Empty,
            RaceDate = finalRace.RaceDate,
            DistanceMeter = finalRace.DistanceMeter,
            MaxLanes = finalRace.MaxLanes,
            Status = finalRace.Status
        };
    }
}
