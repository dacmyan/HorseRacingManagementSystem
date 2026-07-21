using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Application.Features.FinancialRewards.Interfaces;

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

    // Recheck support
    Task<RaceEntry?> GetActiveRaceEntryByRegistrationIdAsync(long registrationId);
    Task<Race?> GetRaceByRaceEntryIdAsync(long raceEntryId);
    Task<Registration?> GetRegistrationWithDetailsAsync(long registrationId);
    void UpdateRegistration(Registration registration);
    void UpdateRaceEntry(RaceEntry raceEntry);
    Task<int?> GetOwnerUserIdByRegistrationAsync(long registrationId);
    Task<int?> GetJockeyUserIdByRaceEntryAsync(long raceEntryId);
    Task<List<int>> GetRefereeUserIdsByRaceIdAsync(long raceId);
    Task<List<int>> GetBettorUserIdsByRaceIdAsync(long raceId);
    Task<List<RaceEntry>> GetAssignedRaceEntriesAsync();
    Task<MedicalCheckRecord?> GetLatestByRegistrationIdAsync(long registrationId);
    Task<IEnumerable<Horse>> GetUnhealthyHorsesAsync();
    Task<Horse?> GetHorseByIdAsync(long horseId);
    void UpdateHorse(Horse horse);
    Task<List<int>> GetVeterinarianUserIdsAsync();
    Task<IEnumerable<MedicalCheckRecord>> GetPendingGeneralChecksAsync();
    Task<MedicalCheckRecord?> GetPendingGeneralCheckByHorseIdAsync(long horseId);
    Task<IDbTransaction> BeginTransactionAsync();
    Task<MedicalCheckRecord?> GetLatestByHorseIdAsync(long horseId);
}
