using FluentAssertions;
using Moq;
using Xunit;
using HorseRacing.Application.Common.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.DTOs;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.FinancialRewards.Services;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Application.Features.UserManagement.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Financials;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Tests.Unit;

public class PrizePayoutServiceTests
{
    private readonly Mock<IBetRepository> _betRepository = new();
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepository = new();
    private readonly Mock<IPrizeRepository> _prizeRepository = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEmailService> _emailService = new();

    private PrizePayoutService CreateService() => new(
        _betRepository.Object,
        _walletRepository.Object,
        _transactionRepository.Object,
        _prizeRepository.Object,
        _notificationService.Object,
        _userRepository.Object,
        _emailService.Object);

    private void SetupConfiguredTournament()
    {
        _prizeRepository.Setup(repository => repository.HasTournamentPrizePayoutsAsync(10)).ReturnsAsync(false);
        _betRepository.Setup(repository => repository.GetTournamentByIdAsync(10))
            .ReturnsAsync(new Tournament { TournamentId = 10, Name = "Test Tournament" });
        _prizeRepository.Setup(repository => repository.GetByTournamentAndRankAsync(10, 1))
            .ReturnsAsync(new Prize { TournamentId = 10, RankPosition = 1, Amount = 3_000_000m });
        _prizeRepository.Setup(repository => repository.GetByTournamentAndRankAsync(10, 2))
            .ReturnsAsync(new Prize { TournamentId = 10, RankPosition = 2, Amount = 2_000_000m });
        _prizeRepository.Setup(repository => repository.GetByTournamentAndRankAsync(10, 3))
            .ReturnsAsync(new Prize { TournamentId = 10, RankPosition = 3, Amount = 1_000_000m });
        _walletRepository.Setup(repository => repository.GetByUserIdAsync(99))
            .ReturnsAsync(new Wallet { WalletId = 1, UserId = 99, Balance = 10_000_000m });
    }

    [Fact]
    public async Task ProcessPrizePayoutAsync_WhenFinalRaceIsMissing_ShouldFailInsteadOfReturningSuccess()
    {
        SetupConfiguredTournament();
        _betRepository.Setup(repository => repository.GetFinalRaceInTournamentAsync(10)).ReturnsAsync((Race?)null);

        var action = () => CreateService().ProcessPrizePayoutAsync(new PrizePayoutRequest
        {
            TournamentId = 10,
            TriggeredByUserId = 99
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Final Round race was not found*");
        _prizeRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessPrizePayoutAsync_WhenAuthenticatedAdminIsMissing_ShouldFail()
    {
        SetupConfiguredTournament();

        var action = () => CreateService().ProcessPrizePayoutAsync(new PrizePayoutRequest
        {
            TournamentId = 10
        });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*authenticated Admin account is required*");
        _notificationService.Verify(
            service => service.GetActiveUserIdsByRoleAsync(It.IsAny<string>()),
            Times.Never);
    }
}
