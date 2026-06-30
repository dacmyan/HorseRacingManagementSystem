using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HorseRacing.Application.Features.TournamentAndRacing.DTOs;
using HorseRacing.Application.Features.TournamentAndRacing.Interfaces;
using HorseRacing.Application.Features.TournamentAndRacing.Services;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using Moq;
using Xunit;

using HorseRacing.Application.Features.Notifications.Interfaces;

namespace HorseRacing.Tests.Unit;

public class TournamentServiceTests
{
    private readonly Mock<ITournamentRepository> _tournamentRepoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly TournamentService _service;

    public TournamentServiceTests()
    {
        _tournamentRepoMock = new Mock<ITournamentRepository>();
        _notificationMock = new Mock<INotificationService>();
        _service = new TournamentService(_tournamentRepoMock.Object, _notificationMock.Object);
    }

    [Fact]
    public async Task CreateTournamentAsync_ShouldCreateOnlyPreAndFinalRounds_WhenNumberOfRoundsIs2()
    {
        // Arrange
        Tournament? createdTournament = null;
        _tournamentRepoMock.Setup(r => r.AddAsync(It.IsAny<Tournament>()))
            .Callback<Tournament>(t =>
            {
                t.TournamentId = 1;
                createdTournament = t;
            })
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new CreateTournamentRequest
        {
            Name = "Summer Cup",
            NumberOfRounds = 2,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var response = await _service.CreateTournamentAsync(request);

        // Assert
        response.Rounds.Should().HaveCount(2);
        response.Rounds.Select(r => r.Name).Should().Equal("Pre", "Final");
        response.Rounds.Select(r => r.RoundNumber).Should().Equal(1, 2);
        createdTournament!.Rounds.Should().HaveCount(2);
        createdTournament.Rounds.Single(r => r.RoundNumber == 2).Races.Should().ContainSingle()
            .Which.Name.Should().Be("Final Race");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task CreateTournamentAsync_ShouldThrowArgumentException_WhenNumberOfRoundsIsNot2(int numberOfRounds)
    {
        // Arrange
        var request = new CreateTournamentRequest
        {
            Name = "Invalid Cup",
            NumberOfRounds = numberOfRounds,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3)
        };

        // Act
        Func<Task> act = async () => await _service.CreateTournamentAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Tournament must have exactly 2 rounds: Pre and Final. (Parameter 'NumberOfRounds')");
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldSplitPrefinalRacesIntoBalancedGroups_WhenRemainderIsTooSmall()
    {
        // Arrange
        var tournament = BuildTournament();
        var registrations = BuildRegistrations(50);
        var createdRaces = new List<Race>();
        var createdEntries = new List<RaceEntry>();

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(101))
            .ReturnsAsync(new List<Race>());
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(registrations);
        _tournamentRepoMock.Setup(r => r.AddRacesAsync(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(races => createdRaces = races.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<RaceEntry>>()))
            .Callback<IEnumerable<RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        response.Should().HaveCount(5);
        createdRaces.Should().HaveCount(5);
        createdRaces.Should().OnlyContain(r =>
            r.RoundId == 101 &&
            r.Name!.Contains("Pre") &&
            r.DistanceMeter == 1200 &&
            r.MaxLanes == 12 &&
            r.Status == "Scheduled");

        createdEntries.GroupBy(e => e.Race).Select(g => g.Count())
            .Should().Equal(12, 12, 12, 6, 8);

        createdEntries.GroupBy(e => e.Race).Should().OnlyContain(group =>
            group.Select(e => e.LaneNo).SequenceEqual(Enumerable.Range(1, group.Count())));
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldCreatePrefinalRaces_WhenApprovedRegistrationsAreAtMost12()
    {
        // Arrange
        var tournament = BuildTournament();
        var registrations = BuildRegistrations(12);
        var existingFinalRace = new Race
        {
            RaceId = 20,
            RoundId = 102,
            Name = "Final Race",
            DistanceMeter = 1600,
            MaxLanes = 12,
            Status = "Scheduled"
        };
        var createdRaces = new List<Race>();
        var createdEntries = new List<RaceEntry>();

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(101))
            .ReturnsAsync(new List<Race>());
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(registrations);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(102))
            .ReturnsAsync(new List<Race> { existingFinalRace });
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(existingFinalRace.RaceId))
            .ReturnsAsync(new List<RaceEntry>());
        _tournamentRepoMock.Setup(r => r.AddRacesAsync(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(races => createdRaces = races.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<RaceEntry>>()))
            .Callback<IEnumerable<RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        response.Should().HaveCount(1);
        createdRaces.Should().HaveCount(1);
        createdRaces.Should().OnlyContain(r =>
            r.RoundId == 101 &&
            r.Name!.Contains("Pre") &&
            r.MaxLanes == 12 &&
            r.Status == "Scheduled");
        createdEntries.Should().HaveCount(12);
        createdEntries.Should().OnlyContain(e => e.Status == "Confirmed");
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldFillExistingFinalRace_FromTopPrefinalRegistrations()
    {
        // Arrange
        var tournament = BuildTournament();
        var prefinalRaces = new List<Race>
        {
            new() { RaceId = 1, RoundId = 101, Name = "Race 1 (Prefinal)", Status = "Finished" },
            new() { RaceId = 2, RoundId = 101, Name = "Race 2 (Prefinal)", Status = "Finished" }
        };
        var existingFinalRace = new Race
        {
            RaceId = 20,
            RoundId = 102,
            Name = "Final Race",
            DistanceMeter = 1600,
            MaxLanes = 12,
            Status = "Scheduled"
        };
        var topRegistrations = BuildRegistrations(12);
        var createdRaces = new List<Race>();
        var createdEntries = new List<RaceEntry>();

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(101))
            .ReturnsAsync(prefinalRaces);
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(topRegistrations);

        var entriesForRace1 = topRegistrations.Take(6).Select(r => new RaceEntry { RegistrationId = r.RegistrationId }).ToList();
        var entriesForRace2 = topRegistrations.Skip(6).Select(r => new RaceEntry { RegistrationId = r.RegistrationId }).ToList();
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(1)).ReturnsAsync(entriesForRace1);
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(2)).ReturnsAsync(entriesForRace2);

        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(102))
            .ReturnsAsync(new List<Race> { existingFinalRace });
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(existingFinalRace.RaceId))
            .ReturnsAsync(new List<RaceEntry>());
        _tournamentRepoMock.Setup(r => r.GetTopHorsesFromPrefinalAsync(tournament.TournamentId, 101))
            .ReturnsAsync(topRegistrations);
        _tournamentRepoMock.Setup(r => r.AddRacesAsync(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(races => createdRaces = races.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<RaceEntry>>()))
            .Callback<IEnumerable<RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        response.Should().ContainSingle();
        createdRaces.Should().BeEmpty();

        createdEntries.Should().HaveCount(12);
        createdEntries.Should().OnlyContain(e => e.RaceId == existingFinalRace.RaceId && e.Status == "Confirmed");
        createdEntries.Select(e => e.RegistrationId).Should().Equal(topRegistrations.Select(r => r.RegistrationId));
        createdEntries.Select(e => e.LaneNo).Should().Equal(Enumerable.Range(1, 12));
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldThrowInvalidOperationException_WhenFinalRaceAlreadyHasParticipants()
    {
        // Arrange
        var tournament = BuildTournament();

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(101))
            .ReturnsAsync(new List<Race> { new() { RaceId = 1, RoundId = 101 } });
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(new List<Registration> { new() { RegistrationId = 1, HorseId = 1 } });
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(1))
            .ReturnsAsync(new List<RaceEntry> { new() { RegistrationId = 1 } });
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(102))
            .ReturnsAsync(new List<Race> { new() { RaceId = 2, RoundId = 102, Name = "Final Race" } });
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(2))
            .ReturnsAsync(new List<RaceEntry> { new() { RaceEntryId = 1, RaceId = 2, RegistrationId = 1 } });

        // Act
        Func<Task> act = async () => await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Final race already has participants.");
    }

    private static Tournament BuildTournament()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(3);

        return new Tournament
        {
            TournamentId = 99,
            Name = "Summer Cup",
            StartDate = startDate,
            EndDate = endDate,
            Rounds =
            {
                new Round
                {
                    RoundId = 101,
                    TournamentId = 99,
                    Name = "Pre",
                    RoundNumber = 1,
                    StartDate = startDate,
                    EndDate = endDate
                },
                new Round
                {
                    RoundId = 102,
                    TournamentId = 99,
                    Name = "Final",
                    RoundNumber = 2,
                    StartDate = startDate,
                    EndDate = endDate
                }
            }
        };
    }

    private static List<Registration> BuildRegistrations(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Registration
            {
                RegistrationId = i,
                TournamentId = 99,
                HorseId = i,
                Status = "Approved"
            })
            .ToList();
    }
}
