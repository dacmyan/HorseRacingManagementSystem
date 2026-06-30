using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.ContractAndRegistration.Services;

public class JockeyContractService : IJockeyContractService
{
    private readonly IJockeyContractRepository _contractRepository;
    private readonly IHorseRepository _horseRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public JockeyContractService(
        IJockeyContractRepository contractRepository,
        IHorseRepository horseRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        ITournamentRepository tournamentRepository)
    {
        _contractRepository = contractRepository;
        _horseRepository = horseRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _tournamentRepository = tournamentRepository;
    }

    private JockeyContractResponse MapToResponse(JockeyContract contract)
    {
        return new JockeyContractResponse
        {
            Id = contract.ContractId,
            HorseId = (int)contract.HorseId,
            HorseName = contract.Horse?.Name ?? "Unknown Horse",
            TournamentId = contract.TournamentId,
            OwnerId = contract.Horse?.OwnerId ?? 0,
            OwnerName = contract.Horse?.Owner?.FullName ?? "Unknown Owner",
            JockeyId = contract.JockeyId,
            JockeyName = contract.Jockey?.FullName ?? "Unknown Jockey",
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            RentalFee = contract.RentalFee,
            WinningBonusPercentage = contract.WinningBonusPercentage,
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

        var tournament = await _tournamentRepository.GetByIdAsync(request.TournamentId);
        if (tournament == null)
        {
            throw new ArgumentException($"Tournament with ID {request.TournamentId} not found.");
        }
        if (!tournament.StartDate.HasValue || !tournament.EndDate.HasValue)
        {
            throw new InvalidOperationException("Tournament dates are not configured.");
        }

        var contractStart = request.StartDate.Date;
        var contractEnd = request.EndDate.Date;
        var tournamentStart = tournament.StartDate.Value.Date;
        var tournamentEnd = tournament.EndDate.Value.Date;
        if (contractStart < tournamentStart || contractEnd > tournamentEnd)
        {
            throw new ArgumentException(
                $"Contract dates must be within the tournament period ({tournamentStart:yyyy-MM-dd} to {tournamentEnd:yyyy-MM-dd}).");
        }

        // 4. Create or reopen JockeyContract. The DB has a unique key on
        // TournamentId + HorseId + JockeyId, so cancelled/rejected invitations
        // are reused instead of inserting a duplicate row.
        var existingContract = await _contractRepository.GetByTournamentHorseAndJockeyAsync(
            request.TournamentId,
            request.HorseId,
            request.JockeyId);

        JockeyContract contract;
        if (existingContract != null)
        {
            if (existingContract.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A pending invitation already exists for this jockey, horse, and tournament.");
            }
            if (existingContract.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("This jockey already has an active contract for this horse and tournament.");
            }

            existingContract.StartDate = request.StartDate;
            existingContract.EndDate = request.EndDate;
            existingContract.RentalFee = request.RentalFee;
            existingContract.WinningBonusPercentage = request.WinningBonusPercentage;
            existingContract.Status = "Pending";
            existingContract.CreatedAt = DateTime.UtcNow;
            contract = existingContract;
        }
        else
        {
            contract = new JockeyContract
            {
                TournamentId = request.TournamentId,
                HorseId = request.HorseId,
                JockeyId = request.JockeyId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RentalFee = request.RentalFee,
                WinningBonusPercentage = request.WinningBonusPercentage,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _contractRepository.AddAsync(contract);
        }

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
            var existingContract = await _contractRepository.GetActiveContractForHorseAsync((int)contract.HorseId);
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

    public async Task<JockeyContractResponse> CancelContractAsync(int ownerUserId, int contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new ArgumentException($"Jockey contract with ID {contractId} not found.");
        }
        if (contract.Horse?.OwnerId != ownerUserId)
        {
            throw new InvalidOperationException("Access denied. You cannot cancel this contract.");
        }
        if (!contract.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only pending contract invitations can be cancelled.");
        }

        contract.Status = "Cancelled";
        await _contractRepository.SaveChangesAsync();

        var notification = new Notification
        {
            UserId = contract.JockeyId,
            Message = $"Contract proposal for horse '{contract.Horse?.Name ?? "Horse"}' was cancelled by the owner.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        return MapToResponse(contract);
    }
}
