using System;
using System.Threading.Tasks;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Domain.Entities.Financials;

namespace HorseRacing.Application.Features.FinancialRewards.Services;

public class PrizePayoutService : IPrizePayoutService
{
    private readonly IBetRepository _betRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IPrizeRepository _prizeRepository;
    private readonly INotificationService _notificationService;

    public PrizePayoutService(
        IBetRepository betRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPrizeRepository prizeRepository,
        INotificationService notificationService)
    {
        _betRepository = betRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _prizeRepository = prizeRepository;
        _notificationService = notificationService;
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
        var tournament = await _betRepository.GetTournamentByIdAsync(request.TournamentId);
        if (tournament == null)
        {
            throw new ArgumentException($"Tournament with ID {request.TournamentId} not found.");
        }

        // 1. Check system treasury balance before configuring prizes
        decimal totalConfiguredPrizePool = (request.FirstPlacePrize > 0 ? request.FirstPlacePrize : 10000m)
                                         + (request.SecondPlacePrize > 0 ? request.SecondPlacePrize : 5000m)
                                         + (request.ThirdPlacePrize > 0 ? request.ThirdPlacePrize : 2500m);

        var adminUserIds = await _notificationService.GetActiveUserIdsByRoleAsync("Admin");
        int adminUserId = adminUserIds.FirstOrDefault();
        if (adminUserId > 0)
        {
            var adminWallet = await GetOrCreateWalletAsync(adminUserId);
            if (adminWallet.Balance < totalConfiguredPrizePool)
            {
                throw new InvalidOperationException(
                    $"Insufficient system treasury balance to configure prizes. Required prize pool: ${totalConfiguredPrizePool:N2}, Current Treasury Balance: ${adminWallet.Balance:N2}. Please deposit funds into the treasury first."
                );
            }
        }

        // 2. Configure and save First, Second, Third place prizes
        var firstPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 1);
        if (firstPrize == null)
        {
            firstPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 1,
                Amount = request.FirstPlacePrize > 0 ? request.FirstPlacePrize : 10000m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(firstPrize);
        }
        else if (request.FirstPlacePrize > 0)
        {
            firstPrize.Amount = request.FirstPlacePrize;
        }

        var secondPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 2);
        if (secondPrize == null)
        {
            secondPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 2,
                Amount = request.SecondPlacePrize > 0 ? request.SecondPlacePrize : 5000m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(secondPrize);
        }
        else if (request.SecondPlacePrize > 0)
        {
            secondPrize.Amount = request.SecondPlacePrize;
        }

        var thirdPrize = await _prizeRepository.GetByTournamentAndRankAsync(request.TournamentId, 3);
        if (thirdPrize == null)
        {
            thirdPrize = new Prize
            {
                TournamentId = request.TournamentId,
                RankPosition = 3,
                Amount = request.ThirdPlacePrize > 0 ? request.ThirdPlacePrize : 2500m,
                OwnerPercentage = 70m,
                JockeyPercentage = 30m
            };
            await _prizeRepository.AddAsync(thirdPrize);
        }
        else if (request.ThirdPlacePrize > 0)
        {
            thirdPrize.Amount = request.ThirdPlacePrize;
        }

        await _prizeRepository.SaveChangesAsync();

        // 2. Try to get finished final race and published result. If not ready, just exit with configurations saved.
        var finalRace = await _betRepository.GetFinalRaceInTournamentAsync(request.TournamentId);
        if (finalRace == null)
        {
            // Prizes saved, but no finished races to process payouts for. Return gracefully.
            return;
        }

        var result = await _betRepository.GetRaceResultAsync(finalRace.RaceId);
        if (result == null || string.IsNullOrWhiteSpace(result.Winner))
        {
            // Prizes saved, but winner result not published. Return gracefully.
            return;
        }

        // 3. Process payouts for all top 3 ranks — wrapped in a DB transaction for atomicity
        await using var dbTransaction = await _prizeRepository.BeginTransactionAsync();
        try
        {
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
                if (horse == null) continue;

                // Calculate rank bonus
                decimal bonusAmount = 0m;
                if (rank == 1) bonusAmount = bonusPool * 0.50m;
                else if (rank == 2) bonusAmount = bonusPool * 0.30m;
                else if (rank == 3) bonusAmount = bonusPool * 0.20m;

                decimal totalPrizeAmount = prize.Amount + bonusAmount;
                decimal ownerAmount = Math.Round(totalPrizeAmount * (prize.OwnerPercentage / 100m), 2);
                decimal jockeyAmount = Math.Round(totalPrizeAmount * (prize.JockeyPercentage / 100m), 2);

                // --- Pay Owner ---
                var ownerWallet = await GetOrCreateWalletAsync(horse.OwnerId);
                ownerWallet.Balance += ownerAmount;

                // --- Deduct from Admin Treasury Wallet ---
                if (adminUserId > 0)
                {
                    var adminWallet = await GetOrCreateWalletAsync(adminUserId);
                    adminWallet.Balance -= ownerAmount;
                    var adminTransaction = new WalletTransaction
                    {
                        WalletId = adminWallet.WalletId,
                        Amount = -ownerAmount,
                        Type = "Prize_Payout",
                        Description = $"Trực tiếp trao thưởng Top {rank} giải đấu '{tournament.Name}' cho ngựa '{horse.Name}'",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(adminTransaction);
                }

                var ownerDescription = $"Nhận thưởng Top {rank} giải đấu '{tournament.Name}' từ ngựa '{horse.Name}'";
                var ownerTransaction = new WalletTransaction
                {
                    WalletId = ownerWallet.WalletId,
                    Amount = ownerAmount,
                    Type = "Prize_Reward",
                    Description = ownerDescription,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.AddAsync(ownerTransaction);

                var ownerPayoutRecord = new TournamentPrizePayout
                {
                    TournamentId = request.TournamentId,
                    UserId = horse.OwnerId,
                    Amount = ownerAmount,
                    Role = "HorseOwner",
                    CreatedAt = DateTime.UtcNow
                };
                await _prizeRepository.AddTournamentPrizePayoutAsync(ownerPayoutRecord);

                await _notificationService.SendNotificationToUserAsync(
                    horse.OwnerId,
                    "Tournament Prize Awarded",
                    $"Congratulations! Your horse '{horse.Name}' won Rank {rank} in tournament '{tournament.Name}'. You received the Owner prize of {ownerAmount:N2}$ (including bonus of {bonusAmount * (prize.OwnerPercentage / 100m):N2}$). New balance: {ownerWallet.Balance:N2}$.",
                    "Wallet",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/owner/wallet"
                );

                // --- Pay Jockey ---
                int jockeyUserId = 0;
                if (entry.JockeyId.HasValue && entry.JockeyProfile != null)
                {
                    jockeyUserId = entry.JockeyProfile.UserId;
                }

                if (jockeyUserId > 0)
                {
                    var jockeyWallet = await GetOrCreateWalletAsync(jockeyUserId);
                    jockeyWallet.Balance += jockeyAmount;

                    var jockeyDescription = $"Nhận thưởng Top {rank} giải đấu '{tournament.Name}' với tư cách Jockey của ngựa '{horse.Name}'";
                    var jockeyTransaction = new WalletTransaction
                    {
                        WalletId = jockeyWallet.WalletId,
                        Amount = jockeyAmount,
                        Type = "Prize_Reward",
                        Description = jockeyDescription,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(jockeyTransaction);

                    var jockeyPayoutRecord = new TournamentPrizePayout
                    {
                        TournamentId = request.TournamentId,
                        UserId = jockeyUserId,
                        Amount = jockeyAmount,
                        Role = "Jockey",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _prizeRepository.AddTournamentPrizePayoutAsync(jockeyPayoutRecord);

                    await _notificationService.SendNotificationToUserAsync(
                        jockeyUserId,
                        "Tournament Prize Awarded",
                        $"Congratulations! You won Rank {rank} in tournament '{tournament.Name}' as jockey of '{horse.Name}'. You received the Jockey prize of {jockeyAmount:N2}$ (including bonus of {bonusAmount * (prize.JockeyPercentage / 100m):N2}$). New balance: {jockeyWallet.Balance:N2}$.",
                        "Wallet",
                        referenceId: (int)tournament.TournamentId,
                        actionUrl: "/spectator/wallet"
                    );
                }
            }

            tournament.Status = "Completed";

            await _prizeRepository.SaveChangesAsync();

            // Commit the DB transaction — all wallet updates are persisted atomically
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
