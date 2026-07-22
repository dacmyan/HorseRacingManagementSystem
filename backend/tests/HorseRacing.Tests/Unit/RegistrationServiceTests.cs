using FluentAssertions;
using HorseRacing.Application.Features.BettingEngine.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;
using HorseRacing.Application.Features.ContractAndRegistration.Interfaces;
using HorseRacing.Application.Features.ContractAndRegistration.Services;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Application.Features.Notifications.Interfaces;
using HorseRacing.Domain.Entities;
using Moq;
using Xunit;

namespace HorseRacing.Tests.Unit;

public class RegistrationServiceTests
{
    [Fact]
    public async Task ReviewRegistrationAsync_ShouldKeepRegistrationPending_WhenTournamentCapacityIsFull()
    {
        var registration = new Registration
        {
            RegistrationId = 49,
            TournamentId = 7,
            HorseId = 49,
            Status = "Pending"
        };
        var repository = new Mock<IRegistrationRepository>();
        repository.Setup(item => item.GetByIdAsync(registration.RegistrationId)).ReturnsAsync(registration);
        repository.Setup(item => item.HasAcceptedJockeyContractAsync(registration.TournamentId, registration.HorseId))
            .ReturnsAsync(true);
        repository.Setup(item => item.ApproveWithinCapacityAsync(registration.RegistrationId, registration.TournamentId, 48))
            .ReturnsAsync(false);

        var service = new RegistrationService(
            repository.Object,
            Mock.Of<IHorseRepository>(),
            Mock.Of<IBetRepository>(),
            Mock.Of<IJockeyContractRepository>(),
            Mock.Of<INotificationService>());

        var action = () => service.ReviewRegistrationAsync(
            registration.RegistrationId,
            new ReviewRegistrationRequest { Status = "Approved" });

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum of 48 horses*waitlist*");
        registration.Status.Should().Be("Pending");
    }
}
