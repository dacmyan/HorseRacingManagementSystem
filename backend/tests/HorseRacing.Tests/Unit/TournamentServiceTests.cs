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
using HorseRacing.Application.Features.BettingEngine.Interfaces;

namespace HorseRacing.Tests.Unit;

public class TournamentServiceTests
{
    private readonly Mock<ITournamentRepository> _tournamentRepoMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IBettingService> _bettingServiceMock;
    private readonly TournamentService _service;

    public TournamentServiceTests()
    {
        _tournamentRepoMock = new Mock<ITournamentRepository>();
        _notificationMock = new Mock<INotificationService>();
        _bettingServiceMock = new Mock<IBettingService>();
        
        _tournamentRepoMock.Setup(r => r.GetMedicalCheckRecordsForTournamentAsync(It.IsAny<long>()))
            .ReturnsAsync((long tId) => 
                Enumerable.Range(1, 48).Select(i => new MedicalCheckRecord 
                {
                    RegistrationId = i,
                    MedicalResult = "Pass",
                    DopingResult = "Negative"
                }).ToList()
            );

        _tournamentRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Tournament>());

        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<Race>());

        _service = new TournamentService(_tournamentRepoMock.Object, _notificationMock.Object, _bettingServiceMock.Object);
    }

    [Fact]
    public async Task CreateTournamentAsync_ShouldCreateTournamentAndPrizeConfigs_WithPendingRegistrationStatus()
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
            Description = "This is a great tournament.",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(5),
            RegistrationStartDate = DateTime.UtcNow,
            RegistrationEndDate = DateTime.UtcNow.AddHours(12)
        };

        // Act
        var response = await _service.CreateTournamentAsync(request);

        // Assert
        response.Name.Should().Be("Summer Cup");
        response.Description.Should().Be("This is a great tournament.");
        response.Status.Should().Be("PendingRegistration");
        createdTournament.Should().NotBeNull();
        createdTournament!.Name.Should().Be("Summer Cup");
        createdTournament.Status.Should().Be("PendingRegistration");
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldSplitPrefinalRacesIntoGroupsOf12_WhenApprovedRegistrationsAreGreaterThan12()
    {
        // Arrange
        var tournament = BuildTournament();
        var registrations = BuildRegistrations(48);
        var createdRaces = new List<Race>();
        var createdEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();

        _tournamentRepoMock.Setup(r => r.ClearRoundsAndRacesAsync(tournament.TournamentId))
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(registrations);
        _tournamentRepoMock.Setup(r => r.GetActiveJockeyProfileIdsByHorseAsync(tournament.TournamentId, It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new Dictionary<long, int>());
        _tournamentRepoMock.Setup(r => r.AddRoundAsync(It.IsAny<Round>()))
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRacesAsync(It.IsAny<IEnumerable<Race>>()))
            .Callback<IEnumerable<Race>>(races => 
            {
                var list = races.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].RaceId = i + 1;
                }
                createdRaces = list;
            })
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>()))
            .Callback<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        response.Should().HaveCount(4);
        createdRaces.Should().HaveCount(4);
        createdRaces.Should().OnlyContain(r =>
            r.Name!.Contains("Pre") &&
            r.DistanceMeter == 1200 &&
            r.MaxLanes == 12 &&
            r.Status == "Scheduled");

        createdEntries.GroupBy(e => e.RaceId).Select(g => g.Count())
            .Should().Equal(12, 12, 12, 12);

        createdEntries.GroupBy(e => e.RaceId).Should().OnlyContain(group =>
            group.Select(e => e.LaneNo).SequenceEqual(Enumerable.Range(1, group.Count())));
    }

    [Fact]
    public async Task GenerateRacesForTournamentAsync_ShouldCreateFinalRaceDirectly_WhenApprovedRegistrationsAreAtMost12()
    {
        // Arrange
        var tournament = BuildTournament();
        var registrations = BuildRegistrations(12);
        var createdRaces = new List<Race>();
        var createdEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();

        _tournamentRepoMock.Setup(r => r.ClearRoundsAndRacesAsync(tournament.TournamentId))
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetApprovedRegistrationsAsync(tournament.TournamentId))
            .ReturnsAsync(registrations);
        _tournamentRepoMock.Setup(r => r.GetActiveJockeyProfileIdsByHorseAsync(tournament.TournamentId, It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new Dictionary<long, int>());
        _tournamentRepoMock.Setup(r => r.AddRoundAsync(It.IsAny<Round>()))
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceAsync(It.IsAny<Race>()))
            .Callback<Race>(race => createdRaces.Add(race))
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>()))
            .Callback<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateRacesForTournamentAsync(tournament.TournamentId);

        // Assert
        response.Should().HaveCount(1);
        createdRaces.Should().HaveCount(1);
        createdRaces.Should().OnlyContain(r =>
            r.Name == "Final Race" &&
            r.MaxLanes == 12 &&
            r.Status == "Scheduled");
        createdEntries.Should().HaveCount(12);
        createdEntries.Should().OnlyContain(e => e.Status == "Confirmed");
    }

    [Fact]
    public async Task GenerateFinalRaceAsync_ShouldFillFinalRace_FromTopPrefinalRegistrations()
    {
        // Arrange
        var tournament = BuildTournament();
        var preRound = tournament.Rounds.First(r => r.RoundNumber == 1);
        var finalRound = tournament.Rounds.First(r => r.RoundNumber == 2);
        
        var preRaces = new List<Race>
        {
            new() { RaceId = 1, RoundId = preRound.RoundId, Name = "Race 1 (Pre)", Status = "Completed" }
        };
        var finalRaces = new List<Race>(); // not created yet

        var topFinalists = BuildPreRoundFinalists(12);
        var createdEntries = new List<HorseRacing.Domain.Entities.RaceEntry>();

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(preRound.RoundId))
            .ReturnsAsync(preRaces);
        _tournamentRepoMock.Setup(r => r.HasRaceResultsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(true);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(finalRound.RoundId))
            .ReturnsAsync(finalRaces);
        
        _tournamentRepoMock.Setup(r => r.AddRaceAsync(It.IsAny<Race>()))
            .Callback<Race>(r => finalRaces.Add(r))
            .Returns(Task.CompletedTask);
        
        _tournamentRepoMock.Setup(r => r.GetFinalistsFromPreRoundAsync(tournament.TournamentId, preRound.RoundId))
            .ReturnsAsync(topFinalists);
        _tournamentRepoMock.Setup(r => r.GetRaceEntriesByRaceIdAsync(It.IsAny<long>()))
            .ReturnsAsync(new List<HorseRacing.Domain.Entities.RaceEntry>());
        _tournamentRepoMock.Setup(r => r.GetActiveJockeyProfileIdsByHorseAsync(tournament.TournamentId, It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new Dictionary<long, int>());
        
        _tournamentRepoMock.Setup(r => r.AddRaceEntriesAsync(It.IsAny<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>()))
            .Callback<IEnumerable<HorseRacing.Domain.Entities.RaceEntry>>(entries => createdEntries = entries.ToList())
            .Returns(Task.CompletedTask);
        _tournamentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _bettingServiceMock.Setup(b => b.RecalculateRaceOddsAsync(It.IsAny<long>())).Returns(Task.CompletedTask);

        // Act
        var response = await _service.GenerateFinalRaceAsync(tournament.TournamentId);

        // Assert
        response.Name.Should().Be("Final Race");
        createdEntries.Should().HaveCount(12);
        createdEntries.Should().OnlyContain(e => e.Status == "Confirmed");
        createdEntries.Select(e => e.RegistrationId).Should().Equal(topFinalists.Select(f => f.RegistrationId));
        createdEntries.Select(e => e.LaneNo).Should().Equal(Enumerable.Range(1, 12));
    }

    [Fact]
    public async Task GenerateFinalRaceAsync_ShouldThrowInvalidOperationException_WhenFinalRaceIsNotScheduled()
    {
        // Arrange
        var tournament = BuildTournament();
        var preRound = tournament.Rounds.First(r => r.RoundNumber == 1);
        var finalRound = tournament.Rounds.First(r => r.RoundNumber == 2);
        
        var preRaces = new List<Race>
        {
            new() { RaceId = 1, RoundId = preRound.RoundId, Name = "Race 1 (Pre)", Status = "Completed" }
        };
        var finalRace = new Race { RaceId = 2, RoundId = finalRound.RoundId, Name = "Final Race", Status = "Running" };

        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(preRound.RoundId))
            .ReturnsAsync(preRaces);
        _tournamentRepoMock.Setup(r => r.HasRaceResultsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(true);
        _tournamentRepoMock.Setup(r => r.GetRacesByRoundIdAsync(finalRound.RoundId))
            .ReturnsAsync(new List<Race> { finalRace });

        // Act
        Func<Task> act = async () => await _service.GenerateFinalRaceAsync(tournament.TournamentId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot generate Final Race because it has already started or completed.");
    }

    [Fact]
    public async Task CreateTournamentAsync_ShouldThrowArgumentException_WhenOverlappingTournamentExists()
    {
        // Arrange
        _tournamentRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Tournament>
            {
                new Tournament
                {
                    TournamentId = 10,
                    Name = "Overlapping Tournament",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    Status = "Active"
                }
            });

        var request = new CreateTournamentRequest
        {
            Name = "Overlapping Cup",
            RegistrationStartDate = DateTime.UtcNow,
            RegistrationEndDate = DateTime.UtcNow.AddHours(12),
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        Func<Task> act = async () => await _service.CreateTournamentAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*must be at least 1 day apart*");
    }

    [Fact]
    public async Task UpdateTournamentAsync_ShouldThrowArgumentException_WhenOverlappingTournamentExists()
    {
        // Arrange
        var tournament = BuildTournament();
        _tournamentRepoMock.Setup(r => r.GetByIdWithRoundsAsync(tournament.TournamentId))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Tournament>
            {
                new Tournament
                {
                    TournamentId = 10,
                    Name = "Overlapping Tournament",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    Status = "Active"
                }
            });

        var request = new UpdateTournamentRequest
        {
            Name = "Summer Cup Updated",
            RegistrationStartDate = tournament.RegistrationStartDate ?? DateTime.UtcNow,
            RegistrationEndDate = tournament.RegistrationEndDate ?? DateTime.UtcNow.AddHours(12),
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        Func<Task> act = async () => await _service.UpdateTournamentAsync(tournament.TournamentId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*must be at least 1 day apart*");
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

    private static List<HorseRacing.Domain.Entities.RaceEntry> BuildPreRoundFinalists(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new HorseRacing.Domain.Entities.RaceEntry
            {
                RaceEntryId = i,
                RegistrationId = i,
                Registration = new Registration { RegistrationId = i, HorseId = i, Horse = new Horse { HorseId = i, AverageTime = 90.0m } },
                FinishTime = 90.0m + i,
                FinishPosition = i
            })
            .ToList();
    }
}
