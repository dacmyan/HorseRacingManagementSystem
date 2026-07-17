using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Domain.Entities;
using HorseRacing.Application.Features.BettingEngine.Interfaces;

using HorseRacing.Application.Features.Notifications.Interfaces;

namespace HorseRacing.Application.Features.TournamentAndRacing.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IBettingService? _bettingService;
    private readonly INotificationService _notificationService;

    public TournamentService(
        ITournamentRepository tournamentRepository,
        INotificationService notificationService,
        IBettingService? bettingService = null)
    {
        _tournamentRepository = tournamentRepository;
        _notificationService = notificationService;
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

        var comparisonTime = request.RegistrationStartDate.Kind == DateTimeKind.Utc
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
        if (request.RegistrationStartDate < comparisonTime.AddMinutes(-5))
        {
            throw new ArgumentException("Thời gian bắt đầu đăng ký không thể ở quá khứ.");
        }

        if (request.RegistrationEndDate <= request.RegistrationStartDate)
        {
            throw new ArgumentException("Registration end date must be after registration start date.");
        }

        if (request.StartDate < request.RegistrationEndDate)
        {
            throw new ArgumentException("Tournament start date must be on or after registration end date.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(request.EndDate));
        }

        if (await _tournamentRepository.HasOverlappingTournamentAsync(request.StartDate, request.EndDate))
        {
            throw new ArgumentException("The tournament duration overlaps with another existing tournament.");
        }

        var tournament = new Tournament
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            RegistrationStartDate = request.RegistrationStartDate,
            RegistrationEndDate = request.RegistrationEndDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "PendingRegistration"
        };

        await _tournamentRepository.AddAsync(tournament);
        await _tournamentRepository.SaveChangesAsync();

        // Save Prize Configurations
        if (request.Prizes != null && request.Prizes.Any())
        {
            foreach (var p in request.Prizes)
            {
                var prize = new HorseRacing.Domain.Entities.Financials.Prize
                {
                    TournamentId = tournament.TournamentId,
                    RankPosition = p.RankPosition,
                    Amount = p.Amount,
                    OwnerPercentage = p.OwnerPercentage,
                    JockeyPercentage = p.JockeyPercentage
                };
                await _tournamentRepository.AddPrizeAsync(prize);
            }
        }
        else
        {
            // Create default prizes (Champion, Runner-up, Third Place)
            var defaultPrizes = new List<HorseRacing.Domain.Entities.Financials.Prize>
            {
                new() { TournamentId = tournament.TournamentId, RankPosition = 1, Amount = 10000m, OwnerPercentage = 70m, JockeyPercentage = 30m },
                new() { TournamentId = tournament.TournamentId, RankPosition = 2, Amount = 5000m, OwnerPercentage = 70m, JockeyPercentage = 30m },
                new() { TournamentId = tournament.TournamentId, RankPosition = 3, Amount = 2500m, OwnerPercentage = 70m, JockeyPercentage = 30m }
            };
            foreach (var prize in defaultPrizes)
            {
                await _tournamentRepository.AddPrizeAsync(prize);
            }
        }
        await _tournamentRepository.SaveChangesAsync();

        try
        {
            await _notificationService.BroadcastNotificationAsync(
                "New Tournament Open for Registration",
                $"Tournament '{tournament.Name}' starting on {tournament.StartDate:dd/MM/yyyy} is now open for registration.",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Notification Error] Failed to broadcast tournament creation: {ex.Message}");
        }

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

        var comparisonTime = request.RegistrationStartDate.Kind == DateTimeKind.Utc
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
        if (request.RegistrationStartDate != tournament.RegistrationStartDate && request.RegistrationStartDate < comparisonTime.AddMinutes(-5))
        {
            throw new ArgumentException("Thời gian bắt đầu đăng ký không thể ở quá khứ.");
        }

        if (request.RegistrationEndDate <= request.RegistrationStartDate)
        {
            throw new ArgumentException("Registration end date must be after registration start date.");
        }

        if (request.StartDate < request.RegistrationEndDate)
        {
            throw new ArgumentException("Tournament start date must be on or after registration end date.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(request.EndDate));
        }

        if (await _tournamentRepository.HasOverlappingTournamentAsync(request.StartDate, request.EndDate, id))
        {
            throw new ArgumentException("The tournament duration overlaps with another ongoing tournament.");
        }

        tournament.Name = request.Name;
        tournament.Description = request.Description ?? string.Empty;
        tournament.RegistrationStartDate = request.RegistrationStartDate;
        tournament.RegistrationEndDate = request.RegistrationEndDate;
        tournament.StartDate = request.StartDate;
        tournament.EndDate = request.EndDate;
        if (!string.IsNullOrEmpty(request.Status))
        {
            tournament.Status = request.Status;
        }

        _tournamentRepository.Update(tournament);
        await _tournamentRepository.SaveChangesAsync();

        try
        {
            await _notificationService.BroadcastNotificationAsync(
                "Tournament Updated",
                $"Tournament '{tournament.Name}' has updated its information and status changed to '{tournament.Status}'.",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Notification Error] Failed to broadcast tournament update: {ex.Message}");
        }

        return MapToResponse(tournament);
    }

    public async Task<List<TournamentResponse>> GetAllTournamentsAsync()
    {
        var tournaments = await _tournamentRepository.GetAllAsync();
        
        bool anyChanged = false;
        DateTime vietnamNow = VietnamNow;
        foreach (var t in tournaments)
        {
            if ((t.Status == "PendingRegistration" || t.Status == "Registration Open") && 
                t.RegistrationEndDate.HasValue && 
                vietnamNow >= t.RegistrationEndDate.Value)
            {
                t.Status = "PendingScheduling";
                anyChanged = true;
            }
            if (t.Status == "PendingRegistration" && 
                t.RegistrationStartDate.HasValue && 
                vietnamNow >= t.RegistrationStartDate.Value)
            {
                t.Status = "Registration Open";
                anyChanged = true;
            }
            if (t.Status == "Upcoming" && 
                t.StartDate.HasValue && 
                vietnamNow >= t.StartDate.Value)
            {
                t.Status = "Active";
                anyChanged = true;
            }
        }
        if (anyChanged)
        {
            await _tournamentRepository.SaveChangesAsync();
        }

        return tournaments.Select(MapToResponse).ToList();
    }

    public async Task<TournamentResponse?> GetTournamentByIdAsync(long id)
    {
        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(id);
        if (tournament == null)
        {
            return null;
        }

        DateTime vietnamNow = VietnamNow;
        bool changed = false;
        if ((tournament.Status == "PendingRegistration" || tournament.Status == "Registration Open") && 
            tournament.RegistrationEndDate.HasValue && 
            vietnamNow >= tournament.RegistrationEndDate.Value)
        {
            tournament.Status = "PendingScheduling";
            changed = true;
        }
        if (tournament.Status == "PendingRegistration" && 
            tournament.RegistrationStartDate.HasValue && 
            vietnamNow >= tournament.RegistrationStartDate.Value)
        {
            tournament.Status = "Registration Open";
            changed = true;
        }
        if (tournament.Status == "Upcoming" && 
            tournament.StartDate.HasValue && 
            vietnamNow >= tournament.StartDate.Value)
        {
            tournament.Status = "Active";
            changed = true;
        }
        if (changed)
        {
            _tournamentRepository.Update(tournament);
            await _tournamentRepository.SaveChangesAsync();
        }

        return MapToResponse(tournament);
    }

    private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

    public async Task<List<RaceScheduleResponse>> GenerateRacesForTournamentAsync(long tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdWithRoundsAsync(tournamentId);
        if (tournament == null) throw new KeyNotFoundException($"Tournament {tournamentId} not found.");

        DateTime vietnamNow = VietnamNow;

        // Validation 1: RegistrationEndDate must be in the past
        if (tournament.RegistrationEndDate.HasValue && vietnamNow < tournament.RegistrationEndDate.Value)
        {
            throw new InvalidOperationException("Registration period has not ended yet.");
        }

        // Validation 2: Tournament must not have already generated races
        if (string.Equals(tournament.Status, "Upcoming", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tournament.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tournament.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Races have already been generated for this tournament.");
        }

        // Fetch registrations and medical checks to filter qualified horses
        var registrations = await _tournamentRepository.GetApprovedRegistrationsAsync(tournamentId);
        var medicalChecks = await _tournamentRepository.GetMedicalCheckRecordsForTournamentAsync(tournamentId);

        var qualifiedRegistrations = registrations.Where(r => 
        {
            var check = medicalChecks.FirstOrDefault(mc => mc.RegistrationId == r.RegistrationId);
            if (check == null) return false;
            bool isMedicalPassed = string.Equals(check.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) || 
                                   string.Equals(check.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase);
            bool isDopingNegative = !string.Equals(check.DopingResult, "Positive", StringComparison.OrdinalIgnoreCase);
            return isMedicalPassed && isDopingNegative;
        }).ToList();

        // Validation 3: Qualified horse count boundaries (12 to 48)
        int N = qualifiedRegistrations.Count;
        if (N < 12)
        {
            if (registrations.Count < 12)
            {
                throw new InvalidOperationException("Minimum 12 qualified horses are required.");
            }
            bool hasUnchecked = registrations.Any(r => !medicalChecks.Any(mc => mc.RegistrationId == r.RegistrationId));
            if (hasUnchecked)
            {
                throw new InvalidOperationException("Minimum 12 qualified horses are required. Some registered horses have not been medically examined yet.");
            }
            throw new InvalidOperationException("Minimum 12 qualified horses are required. Some horses failed the medical or doping check.");
        }
        if (N > 48)
        {
            throw new InvalidOperationException("Maximum 48 qualified horses are allowed.");
        }

        // Clear any existing rounds/races for this tournament to perform a clean Auto Arrange.
        await _tournamentRepository.ClearRoundsAndRacesAsync(tournamentId);
        // Reload tournament to have clean rounds list
        tournament = await _tournamentRepository.GetByIdWithRoundsAsync(tournamentId);
        if (tournament == null) throw new KeyNotFoundException($"Tournament {tournamentId} not found.");

        var activeJockeys = await _tournamentRepository.GetActiveJockeyProfileIdsByHorseAsync(tournamentId, qualifiedRegistrations.Select(r => r.HorseId)) ?? new Dictionary<long, int>();
        var resultRaces = new List<RaceScheduleResponse>();

        if (N == 12)
        {
            // Case 1: Organize only the Final Round directly
            var finalRound = new Round
            {
                TournamentId = tournamentId,
                Name = "Final",
                RoundNumber = 2,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Status = "Scheduled"
            };
            await _tournamentRepository.AddRoundAsync(finalRound);
            await _tournamentRepository.SaveChangesAsync();

            var finalRace = new HorseRacing.Domain.Entities.Tournaments.Race
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

            var entries = new List<HorseRacing.Domain.Entities.RaceEntry>();
            int lane = 1;
            foreach (var reg in qualifiedRegistrations)
            {
                entries.Add(new HorseRacing.Domain.Entities.RaceEntry
                {
                    RaceId = finalRace.RaceId,
                    RegistrationId = reg.RegistrationId,
                    JockeyId = activeJockeys.TryGetValue(reg.HorseId, out var jockeyId) ? jockeyId : (int?)null,
                    LaneNo = lane++,
                    Status = "Confirmed",
                    WinningProbability = 0.5m,
                    CurrentOdds = 2.0m
                });
            }
            await _tournamentRepository.AddRaceEntriesAsync(entries);
            await _tournamentRepository.SaveChangesAsync();

            resultRaces.Add(new RaceScheduleResponse
            {
                RaceId = finalRace.RaceId,
                RoundId = finalRace.RoundId,
                Name = finalRace.Name ?? string.Empty,
                RaceDate = finalRace.RaceDate,
                DistanceMeter = finalRace.DistanceMeter,
                MaxLanes = finalRace.MaxLanes,
                Status = finalRace.Status
            });
        }
        else
        {
            // Case 2: Organize Pre Round
            var preRound = new Round
            {
                TournamentId = tournamentId,
                Name = "Pre",
                RoundNumber = 1,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Status = "Scheduled"
            };
            await _tournamentRepository.AddRoundAsync(preRound);
            await _tournamentRepository.SaveChangesAsync();

            var registrationsList = qualifiedRegistrations.ToList();
            var maxHorsePerRace = 12;
            var horseGroups = BuildRaceGroups(registrationsList, maxHorsePerRace);

            var newRaces = new List<HorseRacing.Domain.Entities.Tournaments.Race>();
            int raceCounter = 1;
            foreach (var group in horseGroups)
            {
                var race = new HorseRacing.Domain.Entities.Tournaments.Race
                {
                    RoundId = preRound.RoundId,
                    Name = $"Race {raceCounter} (Pre)",
                    DistanceMeter = 1200,
                    MaxLanes = maxHorsePerRace,
                    Status = "Scheduled",
                    RaceDate = tournament.StartDate ?? DateTime.UtcNow.AddDays(1)
                };
                newRaces.Add(race);
                raceCounter++;
            }
            await _tournamentRepository.AddRacesAsync(newRaces);
            await _tournamentRepository.SaveChangesAsync();

            var newEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();
            for (int i = 0; i < horseGroups.Count; i++)
            {
                var group = horseGroups[i];
                var race = newRaces[i];
                int lane = 1;
                foreach (var reg in group)
                {
                    newEntries.Add(new HorseRacing.Domain.Entities.RaceEntry
                    {
                        RaceId = race.RaceId,
                        RegistrationId = reg.RegistrationId,
                        JockeyId = activeJockeys.TryGetValue(reg.HorseId, out var jockeyId) ? jockeyId : (int?)null,
                        LaneNo = lane++,
                        Status = "Confirmed",
                        WinningProbability = 0.5m,
                        CurrentOdds = 2.0m
                    });
                }
            }
            await _tournamentRepository.AddRaceEntriesAsync(newEntries);
            await _tournamentRepository.SaveChangesAsync();

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
                    Status = r.Status
                });
            }
        }

        // Change tournament status to Upcoming once races are generated
        tournament.Status = "Upcoming";
        _tournamentRepository.Update(tournament);
        await _tournamentRepository.SaveChangesAsync();

        return resultRaces;
    }

    public async Task<QualifiedHorsesResponse> GetQualifiedHorsesAsync(long id)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(id);
        if (tournament == null) throw new KeyNotFoundException($"Tournament {id} not found.");

        var allRegistrations = await _tournamentRepository.GetRegistrationsByTournamentIdAsync(id);
        var approvedRegistrations = allRegistrations.Where(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase)).ToList();

        var medicalChecks = await _tournamentRepository.GetMedicalCheckRecordsForTournamentAsync(id);

        int totalRegistration = allRegistrations.Count;
        int approvedRegistration = approvedRegistrations.Count;

        int medicalPassed = approvedRegistrations.Count(r => 
        {
            var check = medicalChecks.FirstOrDefault(mc => mc.RegistrationId == r.RegistrationId);
            if (check == null) return false;
            return string.Equals(check.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) || 
                   string.Equals(check.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase);
        });

        var qualifiedRegistrations = approvedRegistrations.Where(r => 
        {
            var check = medicalChecks.FirstOrDefault(mc => mc.RegistrationId == r.RegistrationId);
            if (check == null) return false;
            bool isMedicalPassed = string.Equals(check.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase) || 
                                   string.Equals(check.MedicalResult, "Passed", StringComparison.OrdinalIgnoreCase);
            bool isDopingNegative = !string.Equals(check.DopingResult, "Positive", StringComparison.OrdinalIgnoreCase);
            return isMedicalPassed && isDopingNegative;
        }).ToList();

        int qualifiedHorsesCount = qualifiedRegistrations.Count;

        DateTime vietnamNow = VietnamNow;

        bool canAutoArrange = true;
        string? validationMessage = null;

        if (tournament.RegistrationEndDate.HasValue && vietnamNow < tournament.RegistrationEndDate.Value)
        {
            canAutoArrange = false;
            validationMessage = "Registration period has not ended yet.";
        }
        else if (string.Equals(tournament.Status, "Upcoming", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(tournament.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(tournament.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            canAutoArrange = false;
            validationMessage = "Races have already been generated for this tournament.";
        }
        else if (qualifiedHorsesCount < 12)
        {
            canAutoArrange = false;
            if (approvedRegistration < 12)
            {
                validationMessage = "Minimum 12 qualified horses are required.";
            }
            else
            {
                bool hasUnchecked = approvedRegistrations.Any(r => !medicalChecks.Any(mc => mc.RegistrationId == r.RegistrationId));
                if (hasUnchecked)
                {
                    validationMessage = "Minimum 12 qualified horses are required. Some registered horses have not been medically examined yet.";
                }
                else
                {
                    validationMessage = "Minimum 12 qualified horses are required. Some horses failed the medical or doping check.";
                }
            }
        }
        else if (qualifiedHorsesCount > 48)
        {
            canAutoArrange = false;
            validationMessage = "Maximum 48 qualified horses are allowed.";
        }

        return new QualifiedHorsesResponse
        {
            TotalRegistration = totalRegistration,
            ApprovedRegistration = approvedRegistration,
            MedicalPassed = medicalPassed,
            QualifiedHorses = qualifiedHorsesCount,
            CanAutoArrange = canAutoArrange,
            ValidationMessage = validationMessage
        };
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
        return registrations
            .Select((reg, index) => new { reg, index })
            .GroupBy(x => x.index / maxHorsePerRace)
            .Select(g => g.Select(x => x.reg).ToList())
            .ToList();
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
        return new TournamentResponse
        {
            TournamentId = tournament.TournamentId,
            Name = tournament.Name,
            Description = tournament.Description,
            RegistrationStartDate = tournament.RegistrationStartDate,
            RegistrationEndDate = tournament.RegistrationEndDate,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Status = tournament.Status,
            CancelCount = tournament.CancelCount,
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
        var finalRound = rounds.FirstOrDefault(r => r.RoundNumber == 2);

        if (preRound == null)
        {
            if (finalRound != null)
            {
                var checkFinalRaces = await _tournamentRepository.GetRacesByRoundIdAsync(finalRound.RoundId);
                var checkFinalRace = checkFinalRaces.FirstOrDefault();
                if (checkFinalRace != null)
                {
                    var checkFinalEntries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(checkFinalRace.RaceId);
                    if (checkFinalEntries.Any())
                    {
                        throw new InvalidOperationException("This tournament has exactly 12 horses and was directly arranged into the Final Race. Pre Round is not required.");
                    }
                }
            }
            throw new InvalidOperationException("Pre Round (Round 1) does not exist.");
        }

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
