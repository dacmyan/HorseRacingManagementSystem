using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.DTOs;

namespace HorseRacing.Application.Features.MedicalCheck.Interfaces;

public interface IMedicalCheckService
{
    Task<IEnumerable<MedicalCheckResponse>> GetAllAsync();
    Task<MedicalCheckResponse?> GetByIdAsync(long id);
    Task<IEnumerable<MedicalCheckResponse>> GetByRegistrationIdAsync(long registrationId);
    Task<MedicalCheckResponse> CreateAsync(int performedByUserId, CreateMedicalCheckRequest request);
    Task<MedicalCheckResponse> UpdateAsync(long id, UpdateMedicalCheckRequest request);
    Task DeleteAsync(long id);
    Task<IEnumerable<PendingRegistrationResponse>> GetPendingRegistrationsAsync();
}
