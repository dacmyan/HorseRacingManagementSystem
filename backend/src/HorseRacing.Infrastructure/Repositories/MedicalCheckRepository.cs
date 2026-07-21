using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.Interfaces;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
using HorseRacing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Repositories;

public class MedicalCheckRepository : IMedicalCheckRepository
{
    private readonly AppDbContext _context;

    public MedicalCheckRepository(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<MedicalCheckRecord> WithIncludes()
        => _context.MedicalCheckRecords
            .Include(m => m.Registration)
                .ThenInclude(r => r!.Horse)
                    .ThenInclude(h => h!.Owner)
            .Include(m => m.Registration)
                .ThenInclude(r => r!.Tournament)
            .Include(m => m.Veterinarian);

    public async Task<MedicalCheckRecord?> GetByIdAsync(long id)
        => await WithIncludes().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<MedicalCheckRecord>> GetAllAsync()
        => await WithIncludes().OrderByDescending(m => m.CheckedAt).ToListAsync();

    public async Task<IEnumerable<MedicalCheckRecord>> GetByRegistrationIdAsync(long registrationId)
        => await WithIncludes()
            .Where(m => m.RegistrationId == registrationId)
            .OrderByDescending(m => m.CheckedAt)
            .ToListAsync();

    public async Task<MedicalCheckRecord?> GetLatestByRegistrationIdAsync(long registrationId)
        => await WithIncludes()
            .Where(m => m.RegistrationId == registrationId)
            .OrderByDescending(m => m.CheckedAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(MedicalCheckRecord record)
        => await _context.MedicalCheckRecords.AddAsync(record);

    public void Update(MedicalCheckRecord record)
        => _context.MedicalCheckRecords.Update(record);

    public void Delete(MedicalCheckRecord record)
        => _context.MedicalCheckRecords.Remove(record);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Registration>> GetPendingRegistrationsForChecksAsync()
    {
        return await _context.Registrations
            .Include(r => r.Horse)
                .ThenInclude(h => h!.Owner)
            .Include(r => r.Tournament)
            .Where(r => r.Status == "PendingVet")
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }

    // ─── Recheck Support Methods ──────────────────────────────────────────────

    public async Task<RaceEntry?> GetActiveRaceEntryByRegistrationIdAsync(long registrationId)
    {
        var excludedStatuses = new[] { "Withdrawn", "Scratch", "DNF", "Disqualified", "Finished" };
        return await _context.RaceEntries
            .Include(re => re.Race)
            .FirstOrDefaultAsync(re => re.RegistrationId == registrationId
                && !excludedStatuses.Contains(re.Status));
    }

    public async Task<Race?> GetRaceByRaceEntryIdAsync(long raceEntryId)
    {
        var entry = await _context.RaceEntries
            .Include(re => re.Race)
            .FirstOrDefaultAsync(re => re.RaceEntryId == raceEntryId);
        return entry?.Race;
    }

    public async Task<Registration?> GetRegistrationWithDetailsAsync(long registrationId)
    {
        return await _context.Registrations
            .Include(r => r.Horse)
                .ThenInclude(h => h!.Owner)
            .Include(r => r.Tournament)
            .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);
    }

    public void UpdateRegistration(Registration registration)
        => _context.Registrations.Update(registration);

    public void UpdateRaceEntry(RaceEntry raceEntry)
        => _context.RaceEntries.Update(raceEntry);

    public async Task<int?> GetOwnerUserIdByRegistrationAsync(long registrationId)
    {
        var reg = await _context.Registrations
            .AsNoTracking()
            .Include(r => r.Horse)
            .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

        return reg?.Horse?.OwnerId;
    }

    public async Task<int?> GetJockeyUserIdByRaceEntryAsync(long raceEntryId)
    {
        var entry = await _context.RaceEntries
            .AsNoTracking()
            .Include(re => re.JockeyProfile)
            .FirstOrDefaultAsync(re => re.RaceEntryId == raceEntryId);

        return entry?.JockeyProfile?.UserId;
    }

    public async Task<List<int>> GetRefereeUserIdsByRaceIdAsync(long raceId)
    {
        return await (
            from assignment in _context.RaceRefereeAssignments.AsNoTracking()
            join profile in _context.RefereeProfiles.AsNoTracking()
                on assignment.RefereeId equals profile.RefereeId
            where assignment.RaceId == raceId
            select profile.UserId
        ).ToListAsync();
    }

    public async Task<List<int>> GetBettorUserIdsByRaceIdAsync(long raceId)
    {
        return await _context.Bets
            .AsNoTracking()
            .Where(b => b.RaceId == raceId)
            .Select(b => b.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<RaceEntry>> GetAssignedRaceEntriesAsync()
    {
        var excludedStatuses = new[] { "Withdrawn", "Scratch", "DNF", "Disqualified", "Finished" };
        return await _context.RaceEntries
            .AsNoTracking()
            .Include(re => re.Race)
                .ThenInclude(r => r!.Round)
                    .ThenInclude(round => round!.Tournament)
            .Include(re => re.Registration)
                .ThenInclude(r => r!.Horse)
                    .ThenInclude(h => h!.Owner)
            .Include(re => re.Registration)
                .ThenInclude(r => r!.Tournament)
            .Include(re => re.Registration)
                .ThenInclude(r => r!.MedicalCheckRecords)
            .Include(re => re.JockeyProfile)
                .ThenInclude(j => j!.User)
            .Where(re => !excludedStatuses.Contains(re.Status) && 
                         (re.Registration.Status == "Approved" || re.Registration.Status == "Qualified"))
            .OrderBy(re => re.Race!.RaceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Horse>> GetUnhealthyHorsesAsync()
    {
        return await _context.Horses
            .Include(h => h.Owner)
            .Where(h => h.HealthStatus == "Sick" || h.HealthStatus == "Injured")
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Horse?> GetHorseByIdAsync(long horseId)
    {
        return await _context.Horses
            .Include(h => h.Owner)
            .FirstOrDefaultAsync(h => h.HorseId == horseId);
    }

    public void UpdateHorse(Horse horse)
    {
        _context.Horses.Update(horse);
    }

    public async Task<List<int>> GetVeterinarianUserIdsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role != null && u.Role.Name == "Veterinarian")
            .Select(u => u.UserId)
            .ToListAsync();
    }

    public async Task<HorseRacing.Application.Features.FinancialRewards.Interfaces.IDbTransaction> BeginTransactionAsync()
    {
        var tx = await _context.Database.BeginTransactionAsync();
        return new EfDbTransaction(tx);
    }
}
