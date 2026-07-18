using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.ContractAndRegistration.Interfaces;

public interface IRegistrationRepository
{
    Task<Registration?> GetByIdAsync(long id);
    Task<Registration?> GetByHorseIdAndTournamentIdAsync(long horseId, long tournamentId);
    Task<IEnumerable<Registration>> GetByOwnerIdAsync(int ownerUserId);
    Task AddAsync(Registration registration);
    void Update(Registration registration);
    Task SaveChangesAsync();
    Task<List<int>> GetAdminUserIdsAsync();
}
