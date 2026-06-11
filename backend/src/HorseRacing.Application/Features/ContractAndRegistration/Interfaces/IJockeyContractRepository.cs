using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.ContractAndRegistration.Interfaces;

public interface IJockeyContractRepository
{
    Task<JockeyContract?> GetByIdAsync(int id);
    Task<IEnumerable<JockeyContract>> GetByJockeyIdAsync(int jockeyUserId);
    Task<IEnumerable<JockeyContract>> GetByOwnerIdAsync(int ownerUserId);
    Task<JockeyContract?> GetActiveContractForHorseAsync(int horseId);
    Task AddAsync(JockeyContract contract);
    Task SaveChangesAsync();
}
