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
        return regs.Select(MapToResponse);
    }
}
