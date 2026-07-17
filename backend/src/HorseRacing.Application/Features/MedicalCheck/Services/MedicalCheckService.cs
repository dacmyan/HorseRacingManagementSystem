using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.DTOs;
using HorseRacing.Application.Features.MedicalCheck.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.MedicalCheck.Services;

public class MedicalCheckService : IMedicalCheckService
{
    private readonly IMedicalCheckRepository _repository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly INotificationService _notificationService;

    public MedicalCheckService(
        IMedicalCheckRepository repository,
        IRegistrationRepository registrationRepository,
        INotificationService notificationService)
    {
        _repository = repository;
        _registrationRepository = registrationRepository;
        _notificationService = notificationService;
    }

    // ─── Mapping ────────────────────────────────────────────────────────────
    private static MedicalCheckResponse Map(MedicalCheckRecord r) => new()
    {
        Id              = r.Id,
        RegistrationId  = r.RegistrationId,
        HorseName       = r.Registration?.Horse?.Name,
        TournamentName  = r.Registration?.Tournament?.Name,
        UserId          = r.UserId,
        CheckedByName   = r.Veterinarian != null ? (r.Veterinarian.FullName ?? r.Veterinarian.Username) : null,
        CheckType       = r.CheckType,
        Weight          = r.Weight,
        Temperature     = r.Temperature,
        HeartRate       = r.HeartRate,
        DopingResult    = r.DopingResult,
        MedicalResult   = r.MedicalResult,
        FailReason      = r.FailReason,
        Notes           = r.Notes,
        CheckedAt       = r.CheckedAt,
    };

    // ─── Queries ─────────────────────────────────────────────────────────────
    public async Task<IEnumerable<MedicalCheckResponse>> GetAllAsync()
    {
        var records = await _repository.GetAllAsync();
        return records.Select(Map);
    }

    public async Task<MedicalCheckResponse?> GetByIdAsync(long id)
    {
        var record = await _repository.GetByIdAsync(id);
        return record is null ? null : Map(record);
    }

    public async Task<IEnumerable<MedicalCheckResponse>> GetByRegistrationIdAsync(long registrationId)
    {
        var records = await _repository.GetByRegistrationIdAsync(registrationId);
        return records.Select(Map);
    }

    // ─── Commands ────────────────────────────────────────────────────────────
    // ─── Health Threshold Validation (Business Rule) ─────────────────────────
    /// <summary>
    /// A horse can only receive a "Pass" result if ALL of the following vital signs are within safe range:
    /// - Temperature: 37.2 – 38.3 °C (inclusive)
    /// - HeartRate:   28 – 44 bpm (inclusive)
    /// - DopingResult: must be "Negative"
    /// This rule is enforced at both the service layer (server-side) and the frontend (client-side).
    /// </summary>
    private static void ValidatePassEligibility(decimal? temperature, int? heartRate, decimal? weight, string dopingResult)
    {
        bool tempOk   = temperature.HasValue && temperature.Value >= 37.2m && temperature.Value <= 38.3m;
        bool hrOk     = heartRate.HasValue   && heartRate.Value   >= 28    && heartRate.Value   <= 44;
        bool weightOk = weight.HasValue      && weight.Value      >= 300m  && weight.Value      <= 700m;
        bool dopingOk = string.Equals(dopingResult, "Negative", StringComparison.OrdinalIgnoreCase);

        if (!tempOk || !hrOk || !weightOk || !dopingOk)
            throw new ArgumentException(
                "Setting PASS status is not allowed when weight is out of the 300-700kg range or vital/doping signs do not meet the required health standards!");
    }

    public async Task<MedicalCheckResponse> CreateAsync(int performedByUserId, CreateMedicalCheckRequest request)
    {
        if (request.Weight <= 0)
            throw new ArgumentException("Weight must be greater than zero.");

        var validDoping    = new[] { "Negative", "Positive" };
        var validMedical   = new[] { "Pass", "Fail" };
        var validCheckType = new[] { "Initial", "ReCheck" };

        if (!validDoping.Contains(request.DopingResult))
            throw new ArgumentException("DopingResult must be 'Negative' or 'Positive'.");

        if (!validMedical.Contains(request.MedicalResult))
            throw new ArgumentException("MedicalResult must be 'Pass' or 'Fail'.");

        if (!validCheckType.Contains(request.CheckType))
            throw new ArgumentException("CheckType must be 'Initial' or 'ReCheck'.");

        if (request.MedicalResult == "Fail" && string.IsNullOrWhiteSpace(request.FailReason))
            throw new ArgumentException("FailReason is required when MedicalResult is 'Fail'.");

        // ✅ Business Rule: Validate vital signs thresholds before allowing Pass
        if (string.Equals(request.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase))
            ValidatePassEligibility(request.Temperature, request.HeartRate, request.Weight, request.DopingResult);

        // Business Validation: Registration must exist and be PendingVet.
        var registration = await _registrationRepository.GetByIdAsync(request.RegistrationId);
        if (registration == null)
            throw new ArgumentException($"Registration with ID {request.RegistrationId} does not exist.");

        if (!string.Equals(registration.Status, "PendingVet", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Registration must be pending medical check before an initial medical check can be performed.");

        // For Initial checks: only one Initial check allowed per Registration
        if (request.CheckType == "Initial")
        {
            var existing = await _repository.GetByRegistrationIdAsync(request.RegistrationId);
            if (existing != null && existing.Any(r => r.CheckType == "Initial"))
                throw new ArgumentException("An initial medical check record already exists for this registration.");
        }

        var record = new MedicalCheckRecord
        {
            RegistrationId = request.RegistrationId,
            UserId         = performedByUserId,
            CheckType      = request.CheckType,
            Weight         = request.Weight,
            Temperature    = request.Temperature,
            HeartRate      = request.HeartRate,
            DopingResult   = request.DopingResult,
            MedicalResult  = request.MedicalResult,
            FailReason     = request.FailReason,
            Notes          = request.Notes,
            CheckedAt      = DateTime.UtcNow,
        };

        if (registration != null && registration.Horse != null)
        {
            registration.Horse.HealthStatus = request.MedicalResult == "Fail" 
                ? (request.DopingResult == "Positive" ? "Sick" : "Injured")
                : "Healthy";
        }

        if (registration != null)
        {
            registration.Status = request.MedicalResult == "Pass" ? "Pending" : "Rejected";
        }

        await _repository.AddAsync(record);
        await _repository.SaveChangesAsync();

        var populated = await _repository.GetByIdAsync(record.Id);
        return Map(populated ?? record);
    }

    public async Task<MedicalCheckResponse> UpdateAsync(long id, UpdateMedicalCheckRequest request)
    {
        var record = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical check record with ID {id} not found.");

        if (request.Weight.HasValue)
        {
            if (request.Weight.Value <= 0)
                throw new ArgumentException("Weight must be greater than zero.");
            record.Weight = request.Weight.Value;
        }

        if (request.Temperature.HasValue) record.Temperature = request.Temperature;
        if (request.HeartRate.HasValue)   record.HeartRate   = request.HeartRate;
        if (request.Notes is not null)    record.Notes       = request.Notes;

        if (request.DopingResult is not null)
        {
            var valid = new[] { "Negative", "Positive" };
            if (!valid.Contains(request.DopingResult))
                throw new ArgumentException("DopingResult must be 'Negative' or 'Positive'.");
            record.DopingResult = request.DopingResult;
        }

        if (request.MedicalResult is not null)
        {
            var valid = new[] { "Pass", "Fail" };
            if (!valid.Contains(request.MedicalResult))
                throw new ArgumentException("MedicalResult must be 'Pass' or 'Fail'.");

            // ✅ Business Rule: Validate vital signs thresholds before allowing Pass on update
            if (string.Equals(request.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase))
            {
                var effectiveTemp   = request.Temperature ?? record.Temperature;
                var effectiveHr     = request.HeartRate   ?? record.HeartRate;
                var effectiveWeight = request.Weight      ?? record.Weight;
                var effectiveDoping = request.DopingResult ?? record.DopingResult;
                ValidatePassEligibility(effectiveTemp, effectiveHr, effectiveWeight, effectiveDoping);
            }

            record.MedicalResult = request.MedicalResult;
        }

        _repository.Update(record);
        await _repository.SaveChangesAsync();

        var populated = await _repository.GetByIdAsync(record.Id);
        return Map(populated ?? record);
    }

    public async Task DeleteAsync(long id)
    {
        var record = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Medical check record with ID {id} not found.");

        _repository.Delete(record);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<PendingRegistrationResponse>> GetPendingRegistrationsAsync()
    {
        var regs = await _repository.GetPendingRegistrationsForChecksAsync();
        return regs.Select(r => new PendingRegistrationResponse
        {
            RegistrationId = r.RegistrationId,
            HorseName      = r.Horse?.Name ?? string.Empty,
            TournamentName = r.Tournament?.Name ?? string.Empty,
            OwnerName      = r.Horse?.Owner != null ? (r.Horse.Owner.FullName ?? r.Horse.Owner.Username) : string.Empty,
            RegisteredAt   = r.RegisteredAt
        });
    }

    // ─── Re-Examination Workflow ──────────────────────────────────────────────
    public async Task<RecheckResultResponse> PerformRecheckAsync(int vetUserId, RecheckMedicalRequest request)
    {
        // 1. Validate input
        if (request.Weight <= 0)
            throw new ArgumentException("Weight must be greater than zero.");

        var validDoping  = new[] { "Negative", "Positive" };
        var validMedical = new[] { "Pass", "Fail" };

        if (!validDoping.Contains(request.DopingResult))
            throw new ArgumentException("DopingResult must be 'Negative' or 'Positive'.");
        if (!validMedical.Contains(request.MedicalResult))
            throw new ArgumentException("MedicalResult must be 'Pass' or 'Fail'.");
        if (request.MedicalResult == "Fail" && string.IsNullOrWhiteSpace(request.FailReason))
            throw new ArgumentException("FailReason is required when MedicalResult is 'Fail'.");

        // ✅ Business Rule: Validate vital signs thresholds before allowing Pass in recheck
        if (string.Equals(request.MedicalResult, "Pass", StringComparison.OrdinalIgnoreCase))
            ValidatePassEligibility(request.Temperature, request.HeartRate, request.Weight, request.DopingResult);

        // 2. Validate registration exists and is eligible for re-check
        var registration = await _repository.GetRegistrationWithDetailsAsync(request.RegistrationId)
            ?? throw new ArgumentException($"Registration with ID {request.RegistrationId} does not exist.");

        var eligibleStatuses = new[] { "Approved", "Qualified" };
        if (!eligibleStatuses.Any(s => string.Equals(registration.Status, s, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Registration status '{registration.Status}' is not eligible for re-examination. Must be Approved or Qualified.");

        // 3. Create a NEW MedicalCheckRecord (NEVER overwrite previous records)
        var newRecord = new MedicalCheckRecord
        {
            RegistrationId = request.RegistrationId,
            UserId         = vetUserId,
            CheckType      = "ReCheck",
            Weight         = request.Weight,
            Temperature    = request.Temperature,
            HeartRate      = request.HeartRate,
            DopingResult   = request.DopingResult,
            MedicalResult  = request.MedicalResult,
            FailReason     = request.FailReason,
            Notes          = request.Notes,
            CheckedAt      = DateTime.UtcNow,
        };

        await _repository.AddAsync(newRecord);

        // 4. If horse PASSED → no withdrawal needed
        if (request.MedicalResult == "Pass")
        {
            if (registration.Horse != null)
            {
                registration.Horse.HealthStatus = "Healthy";
            }
            await _repository.SaveChangesAsync();
            var passRecord = await _repository.GetByIdAsync(newRecord.Id);
            return new RecheckResultResponse
            {
                MedicalRecord      = Map(passRecord ?? newRecord),
                HorseWithdrawn     = false,
                RegistrationStatus = registration.Status,
                Message            = $"Re-inspection passed. Horse '{registration.Horse?.Name}' continues to compete."
            };
        }

        // 5. Horse FAILED → withdrawal workflow
        if (registration.Horse != null)
        {
            registration.Horse.HealthStatus = request.DopingResult == "Positive" ? "Sick" : "Injured";
        }

        // 5a. Update Registration → Disqualified
        registration.Status = "Disqualified";
        _repository.UpdateRegistration(registration);

        // 5b. Find active RaceEntry
        var raceEntry = await _repository.GetActiveRaceEntryByRegistrationIdAsync(request.RegistrationId);
        var withdrawStatus = "None";
        var withdrawReason = request.FailReason ?? "FailedMedicalReCheck";

        if (raceEntry != null)
        {
            var race = await _repository.GetRaceByRaceEntryIdAsync(raceEntry.RaceEntryId);
            var alreadyFinalStatuses = new[] { "Withdrawn", "Scratch", "DNF", "Disqualified", "Finished" };

            if (race != null && !alreadyFinalStatuses.Any(s => string.Equals(raceEntry.Status, s, StringComparison.OrdinalIgnoreCase)))
            {
                if (string.Equals(race.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
                {
                    // Race in progress → DNF
                    raceEntry.Status         = "DNF";
                    raceEntry.WithdrawReason = withdrawReason;
                    raceEntry.WithdrawTime   = DateTime.UtcNow;
                    withdrawStatus = "DNF";
                }
                else
                {
                    // Race not started → Withdrawn
                    raceEntry.Status         = "Withdrawn";
                    raceEntry.WithdrawReason = withdrawReason;
                    raceEntry.WithdrawTime   = DateTime.UtcNow;
                    withdrawStatus = "Withdrawn";
                }

                _repository.UpdateRaceEntry(raceEntry);
            }
        }

        await _repository.SaveChangesAsync();

        // 6. Send notifications
        var horseName      = registration.Horse?.Name ?? $"RegistrationId #{registration.RegistrationId}";
        var tournamentName = registration.Tournament?.Name ?? string.Empty;
        var failTitle      = "⚠️ Horse disqualified due to medical check";
        var failContent    = $"Horse {horseName} failed the medical re-inspection in tournament {tournamentName}. Reason: {withdrawReason}.";

        // Notify owner
        var ownerId = await _repository.GetOwnerUserIdByRegistrationAsync(request.RegistrationId);
        if (ownerId.HasValue)
            await _notificationService.SendNotificationToUserAsync(
                ownerId.Value, failTitle, failContent, "MedicalWithdrawal", (int?)raceEntry?.RaceEntryId);

        // Notify jockey & referees & bettors (if race entry exists)
        if (raceEntry != null)
        {
            var jockeyId = await _repository.GetJockeyUserIdByRaceEntryAsync(raceEntry.RaceEntryId);
            if (jockeyId.HasValue)
                await _notificationService.SendNotificationToUserAsync(
                    jockeyId.Value, failTitle,
                    $"Horse {horseName} in tournament {tournamentName} has been withdrawn due to failed medical check. Your contract has been affected.",
                    "MedicalWithdrawal", (int?)raceEntry.RaceEntryId);

            var refereeIds = await _repository.GetRefereeUserIdsByRaceIdAsync(raceEntry.RaceId);
            foreach (var refereeId in refereeIds)
                await _notificationService.SendNotificationToUserAsync(
                    refereeId, failTitle, failContent, "MedicalWithdrawal", (int?)raceEntry.RaceEntryId);

            var bettorIds = await _repository.GetBettorUserIdsByRaceIdAsync(raceEntry.RaceId);
            foreach (var bettorId in bettorIds)
                await _notificationService.SendNotificationToUserAsync(
                    bettorId,
                    "⚠️ Bet Update — Horse withdrawn from race",
                    $"Horse {horseName} has been withdrawn from the race in tournament {tournamentName} due to failing the medical check. Please check your bet status.",
                    "BettingUpdate", (int?)raceEntry.RaceId);
        }

        var populated = await _repository.GetByIdAsync(newRecord.Id);
        var actionText = withdrawStatus == "DNF" ? "marked as DNF" : "withdrawn from the race";
        return new RecheckResultResponse
        {
            MedicalRecord      = Map(populated ?? newRecord),
            HorseWithdrawn     = true,
            WithdrawStatus     = withdrawStatus == "None" ? null : withdrawStatus,
            WithdrawReason     = withdrawReason,
            RegistrationStatus = "Disqualified",
            Message            = $"Re-inspection failed. Horse '{horseName}' has been {actionText}."
        };
    }

    // ─── Assigned Race Entries ────────────────────────────────────────────────
    public async Task<IEnumerable<AssignedRaceEntryResponse>> GetAssignedRaceEntriesAsync()
    {
        var entries = await _repository.GetAssignedRaceEntriesAsync();
        return entries.Select(re =>
        {
            var latestCheck = re.Registration?.MedicalCheckRecords?
                .OrderByDescending(m => m.CheckedAt)
                .FirstOrDefault();

            return new AssignedRaceEntryResponse
            {
                RaceEntryId       = re.RaceEntryId,
                RaceId            = re.RaceId,
                RaceName          = re.Race?.Name,
                RaceDate          = re.Race?.RaceDate ?? default,
                RaceStatus        = re.Race?.Status ?? string.Empty,
                LaneNo            = re.LaneNo,
                RaceEntryStatus   = re.Status,
                RegistrationId    = re.RegistrationId,
                HorseName         = re.Registration?.Horse?.Name,
                OwnerName         = re.Registration?.Horse?.Owner != null
                                      ? (re.Registration.Horse.Owner.FullName ?? re.Registration.Horse.Owner.Username)
                                      : null,
                JockeyName        = re.JockeyProfile?.User != null
                                      ? (re.JockeyProfile.User.FullName ?? re.JockeyProfile.User.Username)
                                      : null,
                TournamentName    = re.Registration?.Tournament?.Name,
                LastMedicalResult = latestCheck?.MedicalResult,
                LastCheckType     = latestCheck?.CheckType,
                LastCheckedAt     = latestCheck?.CheckedAt,
            };
        });
    }
}
