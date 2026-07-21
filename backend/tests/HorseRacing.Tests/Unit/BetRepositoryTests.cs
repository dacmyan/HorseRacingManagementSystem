using HorseRacing.Domain.Entities;
using HorseRacing.Infrastructure.Persistence;
using HorseRacing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HorseRacing.Tests.Unit;

public class BetRepositoryTests
{
    [Fact]
    public async Task GetRaceEntriesWithHorseAsync_ReturnsEntriesForRequestedRaceOnly()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        context.Horses.AddRange(
            new Horse { HorseId = 1, Name = "Horse 1" },
            new Horse { HorseId = 2, Name = "Horse 2" },
            new Horse { HorseId = 3, Name = "Horse 3" });
        context.Registrations.AddRange(
            new Registration { RegistrationId = 1, TournamentId = 1, HorseId = 1 },
            new Registration { RegistrationId = 2, TournamentId = 1, HorseId = 2 },
            new Registration { RegistrationId = 3, TournamentId = 2, HorseId = 3 });
        context.RaceEntries.AddRange(
            new RaceEntry { RaceEntryId = 1, RaceId = 100, RegistrationId = 1, FinishPosition = 1 },
            new RaceEntry { RaceEntryId = 2, RaceId = 100, RegistrationId = 2, FinishPosition = 2 },
            new RaceEntry { RaceEntryId = 3, RaceId = 200, RegistrationId = 3, FinishPosition = 1 });
        await context.SaveChangesAsync();

        var repository = new BetRepository(context);

        var entries = (await repository.GetRaceEntriesWithHorseAsync(100)).ToList();

        Assert.Equal(2, entries.Count);
        Assert.All(entries, entry => Assert.Equal(100, entry.RaceId));
    }
}
