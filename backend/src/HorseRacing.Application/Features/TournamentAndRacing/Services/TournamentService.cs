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

        await ValidateTournamentDatesAsync(
            request.RegistrationStartDate, 
            request.RegistrationEndDate, 
            request.StartDate, 
            request.EndDate);

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
            var msg = $"Giải đấu '{tournament.Name}' đã được tạo thành công. Thời gian mở đăng ký bắt đầu từ {tournament.RegistrationStartDate:dd/MM/yyyy HH:mm}.";
            await _notificationService.SendNotificationToRoleAsync("HorseOwner", "Giải đấu mới được tạo", msg, "Tournament", (int)tournament.TournamentId, actionUrl: "/owner/tournaments");
            await _notificationService.SendNotificationToRoleAsync("Jockey", "Giải đấu mới được tạo", msg, "Tournament", (int)tournament.TournamentId, actionUrl: "/jockey/schedule");
            await _notificationService.SendNotificationToRoleAsync("Referee", "Giải đấu mới được tạo", msg, "Tournament", (int)tournament.TournamentId, actionUrl: "/referee/schedule");
            await _notificationService.SendNotificationToRoleAsync("Veterinarian", "Giải đấu mới được tạo", msg, "Tournament", (int)tournament.TournamentId, actionUrl: "/vet/inspections");
            await _notificationService.SendNotificationToRoleAsync("Spectator", "Giải đấu mới được tạo", msg, "Tournament", (int)tournament.TournamentId, actionUrl: $"/spectator/tournaments/{tournament.TournamentId}");
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

        await ValidateTournamentDatesAsync(
            request.RegistrationStartDate, 
            request.RegistrationEndDate, 
            request.StartDate, 
            request.EndDate, 
            id, 
            tournament);

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
            if (string.Equals(tournament.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                var cancelMsg = $"Giải đấu '{tournament.Name}' đã bị hủy.";
                await _notificationService.SendNotificationToRoleAsync("HorseOwner", "Giải đấu bị hủy", cancelMsg, "Tournament", (int)tournament.TournamentId);
                await _notificationService.SendNotificationToRoleAsync("Jockey", "Giải đấu bị hủy", cancelMsg, "Tournament", (int)tournament.TournamentId);
                await _notificationService.SendNotificationToRoleAsync("Veterinarian", "Giải đấu bị hủy", cancelMsg, "Tournament", (int)tournament.TournamentId);
                await _notificationService.SendNotificationToRoleAsync("Spectator", "Giải đấu bị hủy", cancelMsg, "Tournament", (int)tournament.TournamentId);
            }
            else
            {
                await _notificationService.BroadcastNotificationAsync(
                    "Cập nhật giải đấu",
                    $"Giải đấu '{tournament.Name}' đã được cập nhật thông tin.",
                    "Tournament",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
                );
            }
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
        var closedRegistrationTournamentIds = new List<long>();
        DateTime vietnamNow = VietnamNow;
        foreach (var t in tournaments)
        {
            if ((t.Status == "PendingRegistration" || t.Status == "Registration Open") && 
                t.RegistrationEndDate.HasValue && 
                vietnamNow >= t.RegistrationEndDate.Value)
            {
                t.Status = "PendingScheduling";
                closedRegistrationTournamentIds.Add(t.TournamentId);
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
                if (await _tournamentRepository.HasRacesMissingRefereesAsync(t.TournamentId))
                {
                    t.Status = "PendingAdminAttention";
                    try
                    {
                        var adminIds = await _tournamentRepository.GetAdminUserIdsAsync();
                        foreach (var adminId in adminIds)
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                adminId,
                                "Referees Assignment Required",
                                $"Tournament '{t.Name}' does not have full referees assigned for all races. Please assign referees so the tournament can start.",
                                "System",
                                (int)t.TournamentId
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NOTIFICATION ERROR] Failed to notify admins: {ex.Message}");
                    }
                }
                else
                {
                    t.Status = "Active";
                    try
                    {
                        await _notificationService.BroadcastNotificationAsync(
                            "Tournament Started",
                            $"Tournament '{t.Name}' has officially started. Let the races begin!",
                            "Tournament",
                            referenceId: (int)t.TournamentId,
                            actionUrl: $"/spectator/tournaments/{t.TournamentId}"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NOTIFICATION ERROR] Failed to broadcast tournament start: {ex.Message}");
                    }
                }
                anyChanged = true;
            }
        }
        if (anyChanged)
        {
            await _tournamentRepository.SaveChangesAsync();
        }

        // Auto-cancel registrations without accepted jockey for tournaments that just closed registration
        foreach (var tournamentId in closedRegistrationTournamentIds)
        {
            try
            {
                var cancelledRegs = await _tournamentRepository.CancelRegistrationsWithoutJockeyAsync(tournamentId);
                if (cancelledRegs.Count > 0)
                {
                    var ownerGroups = cancelledRegs.GroupBy(c => c.OwnerId);
                    foreach (var group in ownerGroups)
                    {
                        var horseNames = string.Join(", ", group.Select(c => c.HorseName));
                        var tournamentName = group.First().TournamentName;
                        try
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                group.Key,
                                "Đăng ký bị hủy tự động",
                                $"Đăng ký của ngựa [{horseNames}] trong giải đấu '{tournamentName}' đã bị hủy tự động do chưa có jockey được chấp nhận khi đăng ký đóng.",
                                "System",
                                (int)tournamentId,
                                actionUrl: "/owner/registrations"
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send auto-cancel notification to owner {group.Key}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to cancel registrations without jockey for tournament {tournamentId}: {ex.Message}");
            }
        }

        var responses = tournaments.Select(MapToResponse).ToList();
        foreach (var r in responses)
        {
            r.HasMissingReferees = await _tournamentRepository.HasRacesMissingRefereesAsync(r.TournamentId);
        }
        return responses;
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
        bool registrationJustClosed = false;
        if ((tournament.Status == "PendingRegistration" || tournament.Status == "Registration Open") && 
            tournament.RegistrationEndDate.HasValue && 
            vietnamNow >= tournament.RegistrationEndDate.Value)
        {
            tournament.Status = "PendingScheduling";
            registrationJustClosed = true;
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
            if (await _tournamentRepository.HasRacesMissingRefereesAsync(tournament.TournamentId))
            {
                tournament.Status = "PendingAdminAttention";
                try
                {
                    var adminIds = await _tournamentRepository.GetAdminUserIdsAsync();
                    foreach (var adminId in adminIds)
                    {
                        await _notificationService.SendNotificationToUserAsync(
                            adminId,
                            "Referees Assignment Required",
                            $"Tournament '{tournament.Name}' does not have full referees assigned for all races. Please assign referees so the tournament can start.",
                            "System",
                            (int)tournament.TournamentId
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NOTIFICATION ERROR] Failed to notify admins: {ex.Message}");
                }
            }
            else
            {
                tournament.Status = "Active";
                try
                {
                    await _notificationService.BroadcastNotificationAsync(
                        "Tournament Started",
                        $"Tournament '{tournament.Name}' has officially started. Let the races begin!",
                        "Tournament",
                        referenceId: (int)tournament.TournamentId,
                        actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NOTIFICATION ERROR] Failed to broadcast tournament start: {ex.Message}");
                }
            }
            changed = true;
        }
        if (changed)
        {
            _tournamentRepository.Update(tournament);
            await _tournamentRepository.SaveChangesAsync();
        }

        // Auto-cancel registrations without accepted jockey when registration just closed
        if (registrationJustClosed)
        {
            try
            {
                var cancelledRegs = await _tournamentRepository.CancelRegistrationsWithoutJockeyAsync(id);
                if (cancelledRegs.Count > 0)
                {
                    var ownerGroups = cancelledRegs.GroupBy(c => c.OwnerId);
                    foreach (var group in ownerGroups)
                    {
                        var horseNames = string.Join(", ", group.Select(c => c.HorseName));
                        var tournamentName = group.First().TournamentName;
                        try
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                group.Key,
                                "Đăng ký bị hủy tự động",
                                $"Đăng ký của ngựa [{horseNames}] trong giải đấu '{tournamentName}' đã bị hủy tự động do chưa có jockey được chấp nhận khi đăng ký đóng.",
                                "System",
                                (int)id,
                                actionUrl: "/owner/registrations"
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send auto-cancel notification to owner {group.Key}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to cancel registrations without jockey for tournament {id}: {ex.Message}");
            }
        }

        var response = MapToResponse(tournament);
        response.HasMissingReferees = await _tournamentRepository.HasRacesMissingRefereesAsync(id);
        return response;
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
            string.Equals(tournament.Status, "PendingAdminAttention", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tournament.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Races have already been generated for this tournament.");
        }

        // Validation 2b: Prevent generating races if the tournament status is Ongoing, Finished, or Completed
        if (string.Equals(tournament.Status, "Ongoing", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tournament.Status, "Finished", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Giải đấu đã hoặc đang diễn ra. Không được phép gán lại làn đua!");
        }

        // Validation 2c: Prevent generating races if lanes are already assigned or any race is Live/InProgress/Finished/Completed
        var existingRounds = tournament.Rounds.ToList();
        foreach (var round in existingRounds)
        {
            var races = await _tournamentRepository.GetRacesByRoundIdAsync(round.RoundId);
            foreach (var race in races)
            {
                if (string.Equals(race.Status, "Live", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(race.Status, "InProgress", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(race.Status, "Finished", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(race.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Lượt đua này đã hoặc đang bắt đầu diễn ra. Không được phép gán lại làn đua!");
                }

                var entries = await _tournamentRepository.GetRaceEntriesByRaceIdAsync(race.RaceId);
                if (entries.Any(re => re.LaneNo > 0))
                {
                    throw new InvalidOperationException("Lượt đua này đã được xếp làn. Không được phép gán lại làn đua!");
                }
            }
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

        // Send notifications for scheduled races
        try
        {
            // 1. Notify Veterinarians
            await _notificationService.SendNotificationToRoleAsync(
                "Veterinarian",
                "Yêu cầu tái khám y tế",
                $"Admin đã xếp lịch giải đấu '{tournament.Name}', hãy khám lại lần nữa và gửi kết quả lại cho admin.",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: "/vet/inspections"
            );

            // 2. Notify Spectators
            await _notificationService.SendNotificationToRoleAsync(
                "Spectator",
                "Giải đấu đã được xếp lịch",
                $"Hãy đặt cược với giải đấu '{tournament.Name}' đã được xếp lịch.",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
            );

            // 3. Notify qualified Owners
            var approvedOwners = qualifiedRegistrations
                .Select(r => r.Horse.OwnerId)
                .Distinct()
                .ToList();

            foreach (var ownerId in approvedOwners)
            {
                await _notificationService.SendNotificationToUserAsync(
                    ownerId,
                    "Giải đấu đã được xếp lịch",
                    $"Lịch đua cho giải đấu '{tournament.Name}' đã được xếp thành công. Giải đấu bắt đầu vào {tournament.StartDate:dd/MM/yyyy}.",
                    "Tournament",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/owner/registrations"
                );
            }

            // 4. Notify active Jockeys assigned to qualified horses
            var activeJockeyUserIds = activeJockeys.Values.Distinct().ToList();
            foreach (var jockeyUserId in activeJockeyUserIds)
            {
                await _notificationService.SendNotificationToUserAsync(
                    jockeyUserId,
                    "Giải đấu đã được xếp lịch",
                    $"Giải đấu '{tournament.Name}' mà bạn nài ngựa đã được xếp lịch thi đấu.",
                    "Tournament",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/jockey/schedule"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send race scheduling notifications: {ex.Message}");
        }

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
                 string.Equals(tournament.Status, "PendingAdminAttention", StringComparison.OrdinalIgnoreCase) ||
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

        // Send notifications for final race scheduling
        try
        {
            var approvedOwners = top12Finalists
                .Select(f => f.Registration!.Horse.OwnerId)
                .Distinct()
                .ToList();

            foreach (var ownerId in approvedOwners)
            {
                await _notificationService.SendNotificationToUserAsync(
                    ownerId,
                    "Races Scheduled",
                    $"The Final Race for tournament '{tournament.Name}' has been scheduled. Good luck!",
                    "Tournament",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/owner/registrations"
                );
            }

            await _notificationService.BroadcastNotificationAsync(
                "Races Scheduled",
                $"The Final Race for tournament '{tournament.Name}' has been scheduled. Place your bets now to win exciting rewards!",
                "Tournament",
                referenceId: (int)tournament.TournamentId,
                actionUrl: $"/spectator/tournaments/{tournament.TournamentId}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send final race notifications: {ex.Message}");
        }

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

    private async Task ValidateTournamentDatesAsync(
        DateTime registrationStartDate, 
        DateTime registrationEndDate, 
        DateTime startDate, 
        DateTime endDate, 
        long? excludeTournamentId = null,
        Tournament? existingTournament = null)
    {
        var comparisonTime = registrationStartDate.Kind == DateTimeKind.Utc
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

        // Allow 5 minutes buffer for network clock skew, and only check if dates are newly modified/created
        if (existingTournament == null || registrationStartDate != existingTournament.RegistrationStartDate)
        {
            if (registrationStartDate < comparisonTime.AddMinutes(-5))
            {
                throw new ArgumentException("Registration start date cannot be in the past.");
            }
        }
        if (existingTournament == null || registrationEndDate != existingTournament.RegistrationEndDate)
        {
            if (registrationEndDate < comparisonTime.AddMinutes(-5))
            {
                throw new ArgumentException("Registration end date cannot be in the past.");
            }
        }
        if (existingTournament == null || startDate != existingTournament.StartDate)
        {
            if (startDate < comparisonTime.AddMinutes(-5))
            {
                throw new ArgumentException("Tournament start date cannot be in the past.");
            }
        }
        if (existingTournament == null || endDate != existingTournament.EndDate)
        {
            if (endDate < comparisonTime.AddMinutes(-5))
            {
                throw new ArgumentException("Tournament end date cannot be in the past.");
            }
        }

        if (registrationEndDate <= registrationStartDate)
        {
            throw new ArgumentException("Registration end date must be after registration start date.");
        }

        // Ngày bắt đầu giải đấu phải cách ngày đóng đăng ký ít nhất 48 giờ
        if (startDate < registrationEndDate.AddHours(48))
        {
            throw new ArgumentException("Tournament start date must be at least 48 hours after registration end date to allow for scheduling and referee assignment.");
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException("Tournament end date must be after tournament start date.");
        }

        // Kiểm tra khoảng cách tối thiểu 1 ngày trống giữa các giải đấu (không tính giải đã hoàn thành/hủy)
        var tournaments = await _tournamentRepository.GetAllAsync();
        foreach (var t in tournaments)
        {
            if (t.Status == "Completed" || t.Status == "Cancelled" || !t.StartDate.HasValue || !t.EndDate.HasValue)
            {
                continue;
            }

            if (excludeTournamentId.HasValue && t.TournamentId == excludeTournamentId.Value)
            {
                continue;
            }

            // Giải mới B nằm sau giải hiện tại A
            if (startDate.Date >= t.StartDate.Value.Date)
            {
                var minStartDate = t.EndDate.Value.Date.AddDays(2); // Cách ít nhất 1 ngày trống
                if (startDate.Date < minStartDate)
                {
                    throw new ArgumentException($"The racing period of the new tournament must be at least 1 day apart from the end date of tournament '{t.Name}' ({t.EndDate.Value:dd/MM/yyyy}) (can only start from {minStartDate:dd/MM/yyyy}).");
                }
            }
            // Giải mới B nằm trước giải hiện tại A
            else
            {
                var maxEndDate = t.StartDate.Value.Date.AddDays(-2); // Cách ít nhất 1 ngày trống
                if (endDate.Date > maxEndDate)
                {
                    throw new ArgumentException($"The racing period of the new tournament must be at least 1 day apart from the start date of tournament '{t.Name}' ({t.StartDate.Value:dd/MM/yyyy}) (must end on or before {maxEndDate:dd/MM/yyyy}).");
                }
            }
        }
    }
}
