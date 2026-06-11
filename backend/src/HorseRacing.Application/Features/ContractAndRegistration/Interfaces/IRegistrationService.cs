using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.ContractAndRegistration.DTOs;

namespace HorseRacing.Application.Features.ContractAndRegistration.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResponse> RegisterHorseAsync(int ownerUserId, CreateRegistrationRequest request);
    Task<IEnumerable<RegistrationResponse>> GetRegistrationsByOwnerAsync(int ownerUserId);
}
