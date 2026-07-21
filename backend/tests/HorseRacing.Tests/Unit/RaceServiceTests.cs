using System;
using System.Threading.Tasks;
using FluentAssertions;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Domain.Entities;
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

    [Fact]
    public async Task UpdateRaceAsync_ShouldThrowInvalidOperationException_WhenRaceStatusIsFinished()
    {
        // Arrange
        var raceId = 1L;
        var existingRace = new Race
        {
            RaceId = raceId,
            RoundId = 1,
            Name = "Old Race Name",
            Status = "Finished",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1000,
            MaxLanes = 10
        };

        _raceRepoMock.Setup(r => r.GetByIdWithDetailsAsync(raceId))
            .ReturnsAsync(existingRace);

        var request = new UpdateRaceRequest
        {
            Name = "New Race Name",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1200,
            MaxLanes = 8
        };

        // Act
        Func<Task> act = async () => await _service.UpdateRaceAsync(raceId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Race cannot be edited while its status is 'Finished'.");
    }

    [Fact]
    public async Task UpdateRaceAsync_ShouldThrowInvalidOperationException_WhenMaxLanesLowerThanOccupiedLanes()
    {
        // Arrange
        var raceId = 1L;
        var existingRace = new Race
        {
            RaceId = raceId,
            RoundId = 1,
            Name = "Old Race Name",
            Status = "Scheduled",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1000,
            MaxLanes = 10
        };

        _raceRepoMock.Setup(r => r.GetByIdWithDetailsAsync(raceId))
            .ReturnsAsync(existingRace);

        var entries = new System.Collections.Generic.List<RaceEntry>
        {
            new RaceEntry { RaceId = raceId, LaneNo = 6 }
        };

        _raceRepoMock.Setup(r => r.GetRaceEntriesAsync(raceId))
            .ReturnsAsync(entries);

        var request = new UpdateRaceRequest
        {
            Name = "New Race Name",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1200,
            MaxLanes = 5 // lower than occupied lane 6
        };

        // Act
        Func<Task> act = async () => await _service.UpdateRaceAsync(raceId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot set max lanes to 5 because lane 6 is already occupied.");
    }

    [Fact]
    public async Task UpdateRaceAsync_ShouldUpdateRaceDetails_WhenRequestIsValid()
    {
        // Arrange
        var raceId = 1L;
        var existingRace = new Race
        {
            RaceId = raceId,
            RoundId = 1,
            Name = "Old Race Name",
            Status = "Scheduled",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1000,
            MaxLanes = 10
        };

        _raceRepoMock.Setup(r => r.GetByIdWithDetailsAsync(raceId))
            .ReturnsAsync(existingRace);

        _raceRepoMock.Setup(r => r.GetRaceEntriesAsync(raceId))
            .ReturnsAsync(new System.Collections.Generic.List<RaceEntry>());

        _raceRepoMock.Setup(r => r.GetRaceIdsWithHealthIssuesAsync(It.IsAny<System.Collections.Generic.IEnumerable<long>>()))
            .ReturnsAsync(new System.Collections.Generic.HashSet<long>());

        var request = new UpdateRaceRequest
        {
            Name = "Updated Race Name",
            RaceDate = DateTime.UtcNow.AddDays(2),
            DistanceMeter = 1200,
            MaxLanes = 8
        };

        // Act
        var result = await _service.UpdateRaceAsync(raceId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Race Name");
        result.DistanceMeter.Should().Be(1200);
        result.MaxLanes.Should().Be(8);
        _raceRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
