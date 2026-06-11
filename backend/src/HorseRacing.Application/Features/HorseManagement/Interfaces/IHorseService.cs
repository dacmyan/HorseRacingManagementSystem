using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.HorseManagement.DTOs;

namespace HorseRacing.Application.Features.HorseManagement.Interfaces;

public interface IHorseService
{
    Task<IEnumerable<HorseDetailResponse>> GetHorsesByOwnerAsync(int ownerUserId);
    Task<HorseDetailResponse?> GetHorseByIdAsync(int id, int ownerUserId);
    Task<HorseDetailResponse> CreateHorseAsync(int ownerUserId, RegisterHorseRequest request);
    Task<HorseDetailResponse> UpdateHorseAsync(int id, int ownerUserId, UpdateHorseRequest request);
    Task DeleteHorseAsync(int id, int ownerUserId);
}
