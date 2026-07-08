using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.ContractAndRegistration.Services;

public class RegistrationService : IRegistrationService
{
    private static DateTime VietnamNow => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");

    private readonly IRegistrationRepository _registrationRepository;
    private readonly IHorseRepository _horseRepository;
    private readonly IBetRepository _betRepository;

    public RegistrationService(
        IRegistrationRepository registrationRepository,
        IHorseRepository horseRepository,
        IBetRepository betRepository)
    {
        _registrationRepository = registrationRepository;
        _horseRepository = horseRepository;
        _betRepository = betRepository;
    }

    private RegistrationResponse MapToResponse(Registration reg)
    {
        return new RegistrationResponse
        {
            RegistrationId = reg.RegistrationId,
            TournamentId = reg.TournamentId,
            TournamentName = reg.Tournament?.Name ?? "Unknown Tournament",
            HorseId = reg.HorseId,
            HorseName = reg.Horse?.Name ?? "Unknown Horse",
            Status = reg.Status,
            RegisteredAt = reg.RegisteredAt
        };
    }

    public async Task<RegistrationResponse> RegisterHorseAsync(int ownerUserId, CreateRegistrationRequest request)
    {
        // 1. Verify Horse exists and belongs to owner
        var horse = await _horseRepository.GetByIdAsync(request.HorseId);
        if (horse == null)
        {
            throw new ArgumentException($"Horse with ID {request.HorseId} not found.");
        }
        if (horse.OwnerId != ownerUserId)
        {
            throw new InvalidOperationException("Access denied. You do not own this horse.");
        }

        // 2. Verify Tournament exists
        var tournament = await _betRepository.GetTournamentByIdAsync(request.TournamentId);
        if (tournament == null)
        {
            throw new ArgumentException($"Tournament with ID {request.TournamentId} not found.");
        }
        if ((!tournament.RegistrationStartDate.HasValue || tournament.RegistrationStartDate.Value > VietnamNow) &&
            (!tournament.StartDate.HasValue || tournament.StartDate.Value > VietnamNow))
        {
            throw new InvalidOperationException("Tournament has not started yet.");
        }

        var now = VietnamNow;
        if (tournament.RegistrationStartDate.HasValue && now < tournament.RegistrationStartDate.Value)
        {
            throw new InvalidOperationException($"Registration has not started yet. It opens on {tournament.RegistrationStartDate:yyyy-MM-dd HH:mm:ss} UTC.");
        }
        if (tournament.RegistrationEndDate.HasValue && now > tournament.RegistrationEndDate.Value)
        {
            throw new InvalidOperationException("Registration is closed.");
        }

        // 3. Verify horse is not already registered in this tournament
        var existing = await _registrationRepository.GetByHorseIdAndTournamentIdAsync(request.HorseId, request.TournamentId);
        if (existing != null)
        {
            throw new InvalidOperationException($"Horse '{horse.Name}' is already registered for this tournament.");
        }

        // 4. Create Registration
        var registration = new Registration
        {
            TournamentId = request.TournamentId,
            HorseId = request.HorseId,
            Status = "Pending",
            RegisteredAt = DateTime.UtcNow
        };

        await _registrationRepository.AddAsync(registration);
        await _registrationRepository.SaveChangesAsync();

        var populated = await _registrationRepository.GetByIdAsync(registration.RegistrationId);
        return MapToResponse(populated ?? registration);
    }

    public async Task<IEnumerable<RegistrationResponse>> GetRegistrationsByOwnerAsync(int ownerUserId)
    {
        var regs = await _registrationRepository.GetByOwnerIdAsync(ownerUserId);
        var now = VietnamNow;
        var filteredRegs = regs.Where(r => r.Tournament == null || 
            (r.Tournament.RegistrationStartDate.HasValue && r.Tournament.RegistrationStartDate.Value <= now) || 
            (r.Tournament.StartDate.HasValue && r.Tournament.StartDate.Value <= now));
        return filteredRegs.Select(MapToResponse);
    }

    public async Task<RegistrationResponse> ReviewRegistrationAsync(long id, ReviewRegistrationRequest request)
    {
        var registration = await _registrationRepository.GetByIdAsync(id);
        if (registration == null)
        {
            throw new KeyNotFoundException($"Registration with ID {id} not found.");
        }

        if (!registration.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Registration is already '{registration.Status}'. Only 'Pending' registrations can be reviewed.");
        }

        registration.Status = request.Status;
        await _registrationRepository.SaveChangesAsync();

        var populated = await _registrationRepository.GetByIdAsync(id);
        return MapToResponse(populated ?? registration);
    }
}
