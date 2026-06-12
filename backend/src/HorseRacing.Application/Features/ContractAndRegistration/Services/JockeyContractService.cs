using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.ContractAndRegistration.Services;

public class JockeyContractService : IJockeyContractService
{
    private readonly IJockeyContractRepository _contractRepository;
    private readonly IHorseRepository _horseRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;

    public JockeyContractService(
        IJockeyContractRepository contractRepository,
        IHorseRepository horseRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository)
    {
        _contractRepository = contractRepository;
        _horseRepository = horseRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    private JockeyContractResponse MapToResponse(JockeyContract contract)
    {
        return new JockeyContractResponse
        {
            Id = contract.ContractId,
            HorseId = contract.HorseId,
            HorseName = contract.Horse?.Name ?? "Unknown Horse",
            TournamentId = contract.TournamentId,
            OwnerId = contract.Horse?.OwnerId ?? 0,
            OwnerName = contract.Horse?.Owner?.FullName ?? "Unknown Owner",
            JockeyId = contract.JockeyId,
            JockeyName = contract.Jockey?.FullName ?? "Unknown Jockey",
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            CreatedAt = contract.CreatedAt
        };
    }

    public async Task<JockeyContractResponse> SendContractAsync(int ownerUserId, CreateJockeyContract request)
    {
        // 1. Verify horse exists and belongs to owner
        var horse = await _horseRepository.GetByIdAsync(request.HorseId);
        if (horse == null)
        {
            throw new ArgumentException($"Horse with ID {request.HorseId} not found.");
        }
        if (horse.OwnerId != ownerUserId)
        {
            throw new InvalidOperationException("Access denied. You do not own this horse.");
        }

        // 2. Verify Jockey user exists and has Jockey role
        var jockeyUser = await _userRepository.GetByIdAsync(request.JockeyId);
        if (jockeyUser == null || jockeyUser.Role == null || !jockeyUser.Role.Name.Equals("Jockey", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified user is not a valid Jockey.");
        }
        if (!jockeyUser.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The specified Jockey is currently inactive.");
        }

        // 3. Date validation
        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException("Start date must be before end date.");
        }
        if (request.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Start date cannot be in the past.");
        }

        // 4. Create JockeyContract
        var contract = new JockeyContract
        {
            TournamentId = request.TournamentId,
            HorseId = request.HorseId,
            JockeyId = request.JockeyId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _contractRepository.AddAsync(contract);
        await _contractRepository.SaveChangesAsync();

        // 5. Send notification to Jockey
        var notification = new Notification
        {
            UserId = request.JockeyId,
            Message = $"You received a new jockey contract proposal from Owner '{horse.Owner?.FullName ?? "Owner"}' for horse '{horse.Name}'.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // Fetch populated contract for mapped response
        var populated = await _contractRepository.GetByIdAsync(contract.ContractId);
        return MapToResponse(populated ?? contract);
    }

    public async Task<IEnumerable<JockeyContractResponse>> GetContractsForJockeyAsync(int jockeyUserId)
    {
        var contracts = await _contractRepository.GetByJockeyIdAsync(jockeyUserId);
        return contracts.Select(MapToResponse);
    }

    public async Task<IEnumerable<JockeyContractResponse>> GetContractsForOwnerAsync(int ownerUserId)
    {
        var contracts = await _contractRepository.GetByOwnerIdAsync(ownerUserId);
        return contracts.Select(MapToResponse);
    }

    public async Task<JockeyContractResponse> RespondToContractAsync(int jockeyUserId, int contractId, RespondToContractRequest request)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new ArgumentException($"Jockey contract with ID {contractId} not found.");
        }
        if (contract.JockeyId != jockeyUserId)
        {
            throw new InvalidOperationException("Access denied. You cannot respond to this contract.");
        }
        if (!contract.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Contract is already '{contract.Status}' and cannot be responded to.");
        }

        contract.Status = request.Status;

        // If accepting, cancel/expire other active contracts for the same horse
        if (contract.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            var existingContract = await _contractRepository.GetActiveContractForHorseAsync(contract.HorseId);
            if (existingContract != null)
            {
                existingContract.Status = "Expired";
            }
        }

        await _contractRepository.SaveChangesAsync();

        // Notify Owner of response
        var notification = new Notification
        {
            UserId = contract.Horse?.OwnerId ?? 0,
            Message = $"Jockey '{contract.Jockey?.FullName ?? "Jockey"}' responded '{request.Status}' to contract ID {contract.ContractId} for horse '{contract.Horse?.Name ?? "Horse"}'.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return MapToResponse(contract);
    }
}
