using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.OfficiatingAndResults.DTOs;
using HorseRacing.Application.Features.OfficiatingAndResults.Interfaces;
using HorseRacing.Application.Features.OfficiatingAndResults.Services;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using Moq;
using Xunit;
using FluentAssertions;

using HorseRacing.Application.Features.FinancialRewards.Interfaces;
using HorseRacing.Application.Features.BettingEngine.Interfaces;

namespace HorseRacing.Tests.Unit;

public class RaceResultServiceTests
{
    private readonly Mock<IResultRepository> _repoMock;
    private readonly Mock<IBetPayoutService> _payoutMock;
    private readonly Mock<IPredictionService> _predictionMock;
    private readonly RaceResultService _service;

    public RaceResultServiceTests()
    {
        _repoMock = new Mock<IResultRepository>();
        _payoutMock = new Mock<IBetPayoutService>();
        _predictionMock = new Mock<IPredictionService>();
        _service = new RaceResultService(_repoMock.Object, _payoutMock.Object, _predictionMock.Object);
    }

    [Fact]
    public async Task SubmitResultAsync_ShouldThrowKeyNotFoundException_WhenRaceNotFound()
    {
        // Arrange
        var request = new SubmitRaceResultRequest { RaceId = 999, Winner = "Horse A" };
        _repoMock.Setup(r => r.GetRaceByIdAsync(999)).ReturnsAsync((Race?)null);

        // Act
        Func<Task> act = async () => await _service.SubmitResultAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SubmitResultAsync_ShouldThrowInvalidOperationException_WhenRefereeNotAssigned()
    {
        // Arrange
        var request = new SubmitRaceResultRequest { RaceId = 1, Winner = "Horse A", RefereeId = 5 };
        var race = new Race { RaceId = 1, Name = "Race 1" };
        _repoMock.Setup(r => r.GetRaceByIdAsync(1)).ReturnsAsync(race);
        _repoMock.Setup(r => r.GetAssignmentAsync(1, 5)).ReturnsAsync((RaceRefereeAssignment?)null);

        // Act
        Func<Task> act = async () => await _service.SubmitResultAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The referee is not assigned to this race.");
    }

    [Fact]
    public async Task SubmitResultAsync_ShouldThrowKeyNotFoundException_WhenWinnerHorseNotFound()
    {
        // Arrange
        var request = new SubmitRaceResultRequest { RaceId = 1, Winner = "NonExistent" };
        var race = new Race { RaceId = 1, Name = "Race 1" };
        _repoMock.Setup(r => r.GetRaceByIdAsync(1)).ReturnsAsync(race);
        _repoMock.Setup(r => r.GetHorseByIdOrNameAsync("NonExistent")).ReturnsAsync((Horse?)null);

        // Act
        Func<Task> act = async () => await _service.SubmitResultAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SubmitResultAsync_ShouldThrowArgumentException_WhenHorseNotInRaceEntries()
    {
        // Arrange
        var request = new SubmitRaceResultRequest { RaceId = 1, Winner = "Horse A" };
        var race = new Race { RaceId = 1, Name = "Race 1" };
        var horse = new Horse { HorseId = 10, Name = "Horse A" };
        _repoMock.Setup(r => r.GetRaceByIdAsync(1)).ReturnsAsync(race);
        _repoMock.Setup(r => r.GetHorseByIdOrNameAsync("Horse A")).ReturnsAsync(horse);
        _repoMock.Setup(r => r.GetRaceEntryByHorseIdAsync(1, 10)).ReturnsAsync((RaceEntry?)null);

        // Act
        Func<Task> act = async () => await _service.SubmitResultAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Horse 'Horse A' is not entered in race with ID 1.");
    }

    [Fact]
    public async Task SubmitResultAsync_ShouldPersistResult_WhenValidRequest()
    {
        // Arrange
        var request = new SubmitRaceResultRequest { RaceId = 1, Winner = "Horse A" };
        var race = new Race { RaceId = 1, Name = "Race 1", Status = "Scheduled" };
        var horse = new Horse { HorseId = 10, Name = "Horse A" };
        var entry = new RaceEntry { RaceEntryId = 50, RaceId = 1, RegistrationId = 5 };

        _repoMock.Setup(r => r.GetRaceByIdAsync(1)).ReturnsAsync(race);
        _repoMock.Setup(r => r.GetHorseByIdOrNameAsync("Horse A")).ReturnsAsync(horse);
        _repoMock.Setup(r => r.GetRaceEntryByHorseIdAsync(1, 10)).ReturnsAsync(entry);
        _repoMock.Setup(r => r.GetResultByRaceIdAsync(1)).ReturnsAsync((RaceResult?)null);

        // Act
        var response = await _service.SubmitResultAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.RaceId.Should().Be(1);
        response.Winner.Should().Be("Horse A");
        response.HorseId.Should().Be(10);
        response.RaceEntryId.Should().Be(50);
        _repoMock.Verify(r => r.AddResultAsync(It.Is<RaceResult>(rr => rr.RaceId == 1 && rr.Winner == "Horse A")), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PublishResultAsync_ShouldUpdateRaceStatusToFinished()
    {
        // Arrange
        var race = new Race { RaceId = 1, Name = "Race 1", Status = "Scheduled" };
        var result = new RaceResult { RaceId = 1, Winner = "Horse A" };
        _repoMock.Setup(r => r.GetRaceByIdAsync(1)).ReturnsAsync(race);
        _repoMock.Setup(r => r.GetResultByRaceIdAsync(1)).ReturnsAsync(result);

        // Act
        var response = await _service.PublishResultAsync(1);

        // Assert
        response.Should().NotBeNull();
        race.Status.Should().Be("Finished");
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
