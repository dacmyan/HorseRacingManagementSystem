using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.MedicalCheck.Interfaces;

public interface IMedicalCheckRepository
{
    Task<MedicalCheckRecord?> GetByIdAsync(long id);
    Task<IEnumerable<MedicalCheckRecord>> GetAllAsync();
    Task<IEnumerable<MedicalCheckRecord>> GetByRegistrationIdAsync(long registrationId);
    Task AddAsync(MedicalCheckRecord record);
    void Update(MedicalCheckRecord record);
    void Delete(MedicalCheckRecord record);
    Task SaveChangesAsync();
    Task<IEnumerable<Registration>> GetPendingRegistrationsForChecksAsync();
}
