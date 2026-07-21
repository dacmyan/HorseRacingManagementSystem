using System;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Domain.Entities.Financials;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Application.Common.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class PrizePayoutService : IPrizePayoutService
{
    private readonly IBetRepository _betRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public PrizePayoutService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPrizeRepository prizeRepository,
        INotificationService notificationService,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _prizeRepository = prizeRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    private async Task<Wallet> GetOrCreateWalletAsync(int userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0
            };
            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();
        }
        return wallet;
    }

    public async Task ProcessPrizePayoutAsync(PrizePayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.TournamentId <= 0)
            throw new ArgumentException("Tournament ID must be greater than zero.");
        if (await _prizeRepository.HasTournamentPrizePayoutsAsync(request.TournamentId))
            throw new InvalidOperationException("Tournament prizes have already been paid.");

        var tournament = await _betRepository.GetTournamentByIdAsync(request.TournamentId);
        if (tournament == null)
        {
            throw new ArgumentException($"Tournament with ID {request.TournamentId} not found.");
        }

        var firstPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 1);
        var secondPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 2);
        var thirdPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 3);

        decimal firstAmount = request.FirstPlacePrize > 0 ? request.FirstPlacePrize : firstPrize?.Amount ?? 0m;
        decimal secondAmount = request.SecondPlacePrize > 0 ? request.SecondPlacePrize : secondPrize?.Amount ?? 0m;
        decimal thirdAmount = request.ThirdPlacePrize > 0 ? request.ThirdPlacePrize : thirdPrize?.Amount ?? 0m;
        if (firstAmount <= 0 || secondAmount <= 0 || thirdAmount <= 0)
            throw new InvalidOperationException("All three tournament prizes must be configured before payout.");
        if (!(firstAmount > secondAmount && secondAmount > thirdAmount))
            throw new InvalidOperationException("Prize amounts must follow: first place > second place > third place.");

        decimal totalConfiguredPrizePool = firstAmount + secondAmount + thirdAmount;

        int adminUserId = request.TriggeredByUserId ?? 0;
        if (adminUserId <= 0)
            throw new InvalidOperationException("The authenticated Admin account is required for prize payout.");

        var adminWalletForValidation = await GetOrCreateWalletAsync(adminUserId);
        if (adminWalletForValidation.Balance < totalConfiguredPrizePool)
        {
            throw new InvalidOperationException(
                $"Insufficient system treasury balance. Required prize pool: {totalConfiguredPrizePool:N2} VND, Current Treasury Balance: {adminWalletForValidation.Balance:N2} VND."
            );
        }

        var finalRace = await _betRepository.GetFinalRaceInTournamentAsync(request.TournamentId);
        if (finalRace == null)
            throw new InvalidOperationException("The finished Final Round race was not found. Tournament prizes cannot be paid.");

        var result = await _betRepository.GetRaceResultAsync(finalRace.RaceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
            throw new InvalidOperationException("The published Final Round result and winner are required before prize payout.");

        // 2. Configure and save First, Second, Third place prizes
        if (firstPrize == null)
        {
            firstPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 1,
                Amount = firstAmount,
                OwnerPercentage = 100m,
                JockeyPercentage = 0m
            };
            await _prizeRepository.AddAsync(firstPrize);
        }
        else if (request.FirstPlacePrize > 0)
        {
            firstPrize.Amount = request.FirstPlacePrize;
        }
        firstPrize.OwnerPercentage = 100m;
        firstPrize.JockeyPercentage = 0m;

        if (secondPrize == null)
        {
            secondPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 2,
                Amount = secondAmount,
                OwnerPercentage = 100m,
                JockeyPercentage = 0m
            };
            await _prizeRepository.AddAsync(secondPrize);
        }
        else if (request.SecondPlacePrize > 0)
        {
            secondPrize.Amount = request.SecondPlacePrize;
        }
        secondPrize.OwnerPercentage = 100m;
        secondPrize.JockeyPercentage = 0m;

        if (thirdPrize == null)
        {
            thirdPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 3,
                Amount = thirdAmount,
                OwnerPercentage = 100m,
                JockeyPercentage = 0m
            };
            await _prizeRepository.AddAsync(thirdPrize);
        }
        else if (request.ThirdPlacePrize > 0)
        {
            thirdPrize.Amount = request.ThirdPlacePrize;
        }
        thirdPrize.OwnerPercentage = 100m;
        thirdPrize.JockeyPercentage = 0m;

        await _prizeRepository.SaveChangesAsync();

        // Final race and published winner were validated before any prize data was saved.

        // 3. Process payouts for all top 3 ranks — wrapped in a DB transaction for atomicity
        var ownerNotifications = new List<(int UserId, string HorseName, int Rank, decimal Amount, decimal Balance)>();
        var jockeyNotifications = new List<(int UserId, string HorseName, int Rank, decimal Amount)>();
        await using var dbTransaction = await _prizeRepository.BeginTransactionAsync();
        try
        {
            if (await _prizeRepository.HasTournamentPrizePayoutsAsync(request.TournamentId))
                throw new InvalidOperationException("Tournament prizes have already been paid.");

            decimal totalBets = await _betRepository.GetTotalBetsForRaceAsync(finalRace.RaceId);
            decimal totalPayouts = await _betRepository.GetTotalPayoutsForRaceAsync(finalRace.RaceId);
            decimal houseProfit = totalBets - totalPayouts;

            decimal bonusPool = 0m;
            if (houseProfit > 0)
            {
                bonusPool = houseProfit * 0.20m;
            }

            var finalEntries = (await _betRepository.GetRaceEntriesWithHorseAsync(finalRace.RaceId))
                .Where(re => re.FinishPosition.HasValue)
                .OrderBy(re => re.FinishPosition!.Value)
                .ToList();
            var topThree = finalEntries.Where(e => e.FinishPosition is >= 1 and <= 3).ToList();
            if (topThree.Count != 3 || topThree.Select(e => e.FinishPosition).Distinct().Count() != 3)
                throw new InvalidOperationException("Final race must contain exactly one finisher for each position 1, 2, and 3 before payout.");

            var requiredTreasuryAmount = totalConfiguredPrizePool + bonusPool;
            if (adminUserId <= 0)
                throw new InvalidOperationException("No active Admin treasury account was found.");
            var treasuryWallet = await GetOrCreateWalletAsync(adminUserId);
            if (treasuryWallet.Balance < requiredTreasuryAmount)
                throw new InvalidOperationException($"Insufficient treasury balance. Required: {requiredTreasuryAmount:N2} VND; current: {treasuryWallet.Balance:N2} VND.");

            foreach (var entry in finalEntries)
            {
                int rank = entry.FinishPosition!.Value;
                if (rank > 3)
                {
                    var nonTopHorse = entry.Registration?.Horse;
                    if (nonTopHorse != null)
                    {
                        try
                        {
                            await _notificationService.SendNotificationToUserAsync(
                                nonTopHorse.OwnerId,
                                "Tournament Standing Result",
                                $"Your horse '{nonTopHorse.Name}' finished Rank {rank} in tournament '{tournament.Name}'. This rank does not receive a prize reward. Better luck next time!",
                                "Tournament",
                                referenceId: (int)tournament.TournamentId,
                                actionUrl: "/owner/results"
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[NOTIFICATION ERROR] Failed to send ranking notification to owner {nonTopHorse.OwnerId}: {ex.Message}");
                        }
                    }
                    continue;
                }

                if (rank < 1) continue;

                var prize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, rank);
                if (prize == null) continue;

                var horse = entry.Registration?.Horse;
                if (horse == null && entry.Registration != null && entry.Registration.HorseId > 0)
                {
                    horse = await _betRepository.GetHorseByIdAsync(entry.Registration.HorseId);
                }
                if (horse == null) continue;

                // Calculate rank bonus
                decimal bonusAmount = 0m;
                if (rank == 1) bonusAmount = bonusPool * 0.50m;
                else if (rank == 2) bonusAmount = bonusPool * 0.30m;
                else if (rank == 3) bonusAmount = bonusPool * 0.20m;

                decimal totalPrizeAmount = Math.Round(prize.Amount + bonusAmount, 2);

                // --- 100% Prize Payout goes to Horse Owner's Wallet ---
                var ownerWallet = await GetOrCreateWalletAsync(horse.OwnerId);
                ownerWallet.Balance += totalPrizeAmount;

                // --- Deduct from Admin Treasury Wallet ---
                if (adminUserId > 0)
                {
                    var adminWallet = await GetOrCreateWalletAsync(adminUserId);
                    adminWallet.Balance -= totalPrizeAmount;
                    var adminTransaction = new WalletTransaction
                    {
                        WalletId = adminWallet.WalletId,
                        Amount = -totalPrizeAmount,
                        Type = "Prize_Payout",
                        Description = $"Awarded Top {rank} prize for tournament '{tournament.Name}' to horse '{horse.Name}'",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(adminTransaction);
                }

                var ownerDescription = $"Received Top {rank} prize for tournament '{tournament.Name}' from horse '{horse.Name}'";
                var ownerTransaction = new WalletTransaction
                {
                    WalletId = ownerWallet.WalletId,
                    Amount = totalPrizeAmount,
                    Type = "Prize_Reward",
                    Description = ownerDescription,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(ownerTransaction);

                var ownerPayoutRecord = new TournamentPrizePayout
                {
                    TournamentId = request.TournamentId,
                    UserId = horse.OwnerId,
                    Amount = totalPrizeAmount,
                    Role = "HorseOwner",
                    CreatedAt = DateTime.UtcNow
                };
                await _prizeRepository.AddTournamentPrizePayoutAsync(ownerPayoutRecord);

                ownerNotifications.Add((horse.OwnerId, horse.Name, rank, totalPrizeAmount, ownerWallet.Balance));

                // --- Jockey Notification (No money transferred to Jockey wallet) ---
                int jockeyUserId = 0;
                if (entry.JockeyId.HasValue && entry.JockeyProfile != null)
                {
                    jockeyUserId = entry.JockeyProfile.UserId;
                }

                if (jockeyUserId > 0)
                {
                    jockeyNotifications.Add((jockeyUserId, horse.Name, rank, totalPrizeAmount));
                }
            }

            tournament.Status = "Completed";

            await _prizeRepository.SaveChangesAsync();

            // Commit the DB transaction — all wallet updates are persisted atomically
            await dbTransaction.CommitAsync();

            // Send notifications only after the wallet transfer and payout records
            // have committed successfully.
            foreach (var notice in ownerNotifications)
            {
                try
                {
                    await _notificationService.SendNotificationToUserAsync(
                        notice.UserId,
                        "Tournament Prize Payout",
                        $"Congratulations! Your horse '{notice.HorseName}' achieved Top {notice.Rank} in tournament '{tournament.Name}'. You received {notice.Amount:N2} VND. Current wallet balance: {notice.Balance:N2} VND.",
                        "Wallet",
                        referenceId: (int)tournament.TournamentId,
                        actionUrl: "/owner/results");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NOTIFICATION ERROR] Failed to notify owner {notice.UserId}: {ex.Message}");
                }
            }

            foreach (var notice in jockeyNotifications)
            {
                try
                {
                    await _notificationService.SendNotificationToUserAsync(
                        notice.UserId,
                        "Outstanding Jockey Performance",
                        $"Congratulations! Horse '{notice.HorseName}', which you rode, achieved Top {notice.Rank} in tournament '{tournament.Name}' with a total prize of {notice.Amount:N2} VND.",
                        "Tournament",
                        referenceId: (int)tournament.TournamentId,
                        actionUrl: "/jockey/schedule");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NOTIFICATION ERROR] Failed to notify jockey {notice.UserId}: {ex.Message}");
                }
            }

            // Broadcast email to users
            try
            {
                var allUsers = await _userRepository.GetAllUsersAsync();
                var targetUsers = allUsers.Where(u => u.Role?.Name != "Admin" && u.Role?.Name != "Veterinarian" && !string.IsNullOrEmpty(u.Email)).ToList();

                string topHorsesHtml = "";
                foreach (var entry in finalEntries)
                {
                    int rank = entry.FinishPosition!.Value;
                    if (rank < 1 || rank > 3) continue;

                    var prize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, rank);
                    if (prize == null) continue;

                    var horseName = entry.Registration?.Horse?.Name ?? "Unknown";

                    decimal bonusAmount = 0m;
                    if (rank == 1) bonusAmount = bonusPool * 0.50m;
                    else if (rank == 2) bonusAmount = bonusPool * 0.30m;
                    else if (rank == 3) bonusAmount = bonusPool * 0.20m;

                    decimal totalPrize = prize.Amount + bonusAmount;

                    topHorsesHtml += $"<li><strong>Top {rank}:</strong> {horseName} - Prize: {totalPrize:N2}$</li>";
                }

                string emailSubject = $"Tournament results for {tournament.Name} officially published!";
                string emailBody = $@"
                    <h2>Tournament {tournament.Name} has concluded!</h2>
                    <p>Hello,</p>
                    <p>Tournament <strong>{tournament.Name}</strong> has officially ended. Here are the winning horses that placed in the top ranks:</p>
                    <ul>
                        {topHorsesHtml}
                    </ul>
                    <p>Thank you for choosing Horse Racing Management System.</p>
                ";

                var emailTasks = targetUsers.Select(u => _emailService.SendEmailAsync(u.Email, emailSubject, emailBody));
                await Task.WhenAll(emailTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Broadcast Error] Failed to broadcast tournament results: {ex.Message}");
            }
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
