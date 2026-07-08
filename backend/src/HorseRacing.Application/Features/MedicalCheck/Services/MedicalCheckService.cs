using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.DTOs;
using HorseRacing.Application.Features.MedicalCheck.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.MedicalCheck.Services;

public class MedicalCheckService : IMedicalCheckService
{
    private readonly IMedicalCheckRepository _repository;
    private readonly IRegistrationRepository _registrationRepository;

    public MedicalCheckService(
        IMedicalCheckRepository repository,
        IRegistrationRepository registrationRepository)
    {
        _repository = repository;
        _registrationRepository = registrationRepository;
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
        Weight          = r.Weight,
        Temperature     = r.Temperature,
        HeartRate       = r.HeartRate,
        DopingResult    = r.DopingResult,
        MedicalResult   = r.MedicalResult,
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
    public async Task<MedicalCheckResponse> CreateAsync(int performedByUserId, CreateMedicalCheckRequest request)
    {
        if (request.Weight <= 0)
            throw new ArgumentException("Weight must be greater than zero.");

        var validDoping  = new[] { "Negative", "Positive" };
        var validMedical = new[] { "Pass", "Fail" };

        if (!validDoping.Contains(request.DopingResult))
            throw new ArgumentException("DopingResult must be 'Negative' or 'Positive'.");

        if (!validMedical.Contains(request.MedicalResult))
            throw new ArgumentException("MedicalResult must be 'Pass' or 'Fail'.");

        // Business Validation: Registration must exist and be Approved.
        var registration = await _registrationRepository.GetByIdAsync(request.RegistrationId);
        if (registration == null)
            throw new ArgumentException($"Registration with ID {request.RegistrationId} does not exist.");

        if (!string.Equals(registration.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Registration must be approved before a medical check can be performed.");

        // Unique Constraint check: Only one MedicalCheckRecord per Registration.
        var existingRecords = await _repository.GetByRegistrationIdAsync(request.RegistrationId);
        if (existingRecords != null && existingRecords.Any())
            throw new ArgumentException("A medical check record already exists for this registration.");

        var record = new MedicalCheckRecord
        {
            RegistrationId = request.RegistrationId,
            UserId         = performedByUserId,
            Weight         = request.Weight,
            Temperature    = request.Temperature,
            HeartRate      = request.HeartRate,
            DopingResult   = request.DopingResult,
            MedicalResult  = request.MedicalResult,
            Notes          = request.Notes,
            CheckedAt      = DateTime.UtcNow,
        };

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
            HorseName = r.Horse?.Name ?? string.Empty,
            TournamentName = r.Tournament?.Name ?? string.Empty,
            OwnerName = r.Horse?.Owner != null ? (r.Horse.Owner.FullName ?? r.Horse.Owner.Username) : string.Empty,
            RegisteredAt = r.RegisteredAt
        });
    }
}
