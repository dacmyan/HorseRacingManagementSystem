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
                        Description = $"Trực tiếp trao thưởng Top {rank} giải đấu '{tournament.Name}' cho ngựa '{horse.Name}'",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _transactionRepository.AddAsync(adminTransaction);
                }



                var ownerDescription = $"Nhận thưởng Top {rank} giải đấu '{tournament.Name}' từ ngựa '{horse.Name}'";
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

                // Send notification to Owner with horse name, rank, and prize amount
                await _notificationService.SendNotificationToUserAsync(
                    horse.OwnerId,
                    "Trao thưởng Giải đấu",
                    $"Chúc mừng! Ngựa '{horse.Name}' của bạn đã xuất sắc đạt xếp hạng Top {rank} trong giải đấu '{tournament.Name}' và bạn đã nhận được số tiền thưởng là {totalPrizeAmount:N2}$ vào ví. Số dư ví hiện tại: {ownerWallet.Balance:N2}$.",
                    "Wallet",
                    referenceId: (int)tournament.TournamentId,
                    actionUrl: "/owner/wallet"
                );

                // --- Jockey Notification (No money transferred to Jockey wallet) ---
                int jockeyUserId = 0;
                if (entry.JockeyId.HasValue && entry.JockeyProfile != null)
                {
                    jockeyUserId = entry.JockeyProfile.UserId;
                }

                if (jockeyUserId > 0)
                {
                    // Only send achievement notification to Jockey whose horse placed in Top 3
                    await _notificationService.SendNotificationToUserAsync(
                        jockeyUserId,
                        "Kết quả nài ngựa xuất sắc",
                        $"Chúc mừng! Ngựa '{horse.Name}' mà bạn nài đã xuất sắc đạt xếp hạng Top {rank} trong giải đấu '{tournament.Name}' với tổng số tiền thưởng giải đấu đạt {totalPrizeAmount:N2}$.",
                        "Tournament",
                        referenceId: (int)tournament.TournamentId,
                        actionUrl: "/jockey/schedule"
                    );
                }
            }

            tournament.Status = "Completed";

            await _prizeRepository.SaveChangesAsync();

            // Commit the DB transaction — all wallet updates are persisted atomically
            await dbTransaction.CommitAsync();

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

                string emailSubject = $"Kết quả giải đấu {tournament.Name} đã chính thức công bố!";
                string emailBody = $@"
                    <h2>Giải đấu {tournament.Name} đã kết thúc!</h2>
                    <p>Xin chào,</p>
                    <p>Giải đấu <strong>{tournament.Name}</strong> đã chính thức khép lại. Dưới đây là những chú ngựa xuất sắc nhất đã giành chiến thắng:</p>
                    <ul>
                        {topHorsesHtml}
                    </ul>
                    <p>Cảm ơn bạn đã luôn đồng hành cùng Horse Racing Management System.</p>
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
