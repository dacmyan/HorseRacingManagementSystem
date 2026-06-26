using System;
using System.Threading.Tasks;
using FluentAssertions;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Domain.Entities.Tournaments;
using Moq;
using Xunit;

namespace HorseRacing.Tests.Unit;

public class RaceServiceTests
{
    private readonly Mock<IRaceRepository> _raceRepoMock;
    private readonly RaceService _service;

    public RaceServiceTests()
    {
        _raceRepoMock = new Mock<IRaceRepository>();
        _service = new RaceService(_raceRepoMock.Object);
    }

    [Fact]
    public async Task CreateRaceAsync_ShouldThrowArgumentException_WhenMaxLanesExceeds12()
    {
        // Arrange
        _raceRepoMock.Setup(r => r.GetRoundByIdAsync(1))
            .ReturnsAsync(new Round
            {
                RoundId = 1,
                TournamentId = 10,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3)
            });

        var request = new CreateRaceRequest
        {
            RoundId = 1,
            Name = "Pre Race 1",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1200,
            MaxLanes = 13
        };

        // Act
        Func<Task> act = async () => await _service.CreateRaceAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Max lanes cannot exceed 12. (Parameter 'MaxLanes')");
    }
}
