using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HorseRacing.Application.Features.MedicalCheck.Interfaces;
using HorseRacing.Domain.Entities;
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
        var checkedRegIds = await _context.MedicalCheckRecords
            .Select(m => m.RegistrationId)
            .ToListAsync();

        return await _context.Registrations
            .Include(r => r.Horse)
                .ThenInclude(h => h!.Owner)
            .Include(r => r.Tournament)
            .Where(r => r.Status == "Approved" && !checkedRegIds.Contains(r.RegistrationId))
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }
}
