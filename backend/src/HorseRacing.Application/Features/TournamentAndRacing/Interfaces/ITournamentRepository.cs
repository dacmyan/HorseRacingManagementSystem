using System.Threading.Tasks;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Application.Features.TournamentAndRacing.Interfaces;

public interface ITournamentRepository
{
    Task AddAsync(Tournament tournament);
    Task SaveChangesAsync();
}
