using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;

namespace HorseRacing.Infrastructure.Persistence;

public class MainflowDataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<MainflowDataSeeder> _logger;

    public MainflowDataSeeder(AppDbContext context, ILogger<MainflowDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting Mainflow data seeding (Dac Tour 3)...");

        try
        {
            // Ensure owner3 exists
            var owner3Username = "owner3";
            var owner3User = await _context.Users.FirstOrDefaultAsync(u => u.Username == owner3Username);
            if (owner3User == null)
            {
                _logger.LogWarning("Owner 'owner3' not found. Make sure DataSeeder runs first.");
                return;
            }

            // 1. Seed Tournament "Dac Tour 3"
            var dacTour3Name = "Dac Tour 3";
            var dacTour3 = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == dacTour3Name);
            if (dacTour3 == null)
            {
                dacTour3 = new Tournament
                {
                    Name = dacTour3Name,
                    Description = "Dac Tour 3 Tournament with accepted jockey contracts",
                    RegistrationStartDate = DateTime.UtcNow.AddDays(-10),
                    RegistrationEndDate = DateTime.UtcNow.AddDays(5),
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(20),
                    Status = "Registration Open"
                };
                _context.Tournaments.Add(dacTour3);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tournament '{dacTour3Name}' seeded successfully in MainflowDataSeeder.");
            }

            for (int i = 1; i <= 12; i++)
            {
                var horseName = $"Owner3-Horse{i}";
                var jockeyUsername = $"jockeytest{i}"; // Matching the seeded username jockeytest#

                var horse = await _context.Horses.FirstOrDefaultAsync(h => h.Name == horseName);
                var jockey = await _context.Users.FirstOrDefaultAsync(u => u.Username == jockeyUsername);

                if (horse != null && jockey != null)
                {
                    // 2. Seed JockeyContract invitation with status 'Accepted'
                    var contract = await _context.JockeyContracts.FirstOrDefaultAsync(jc => jc.TournamentId == dacTour3.TournamentId && jc.HorseId == horse.HorseId && jc.JockeyId == jockey.UserId);
                    if (contract == null)
                    {
                        contract = new JockeyContract
                        {
                            TournamentId = dacTour3.TournamentId,
                            HorseId = horse.HorseId,
                            JockeyId = jockey.UserId,
                            StartDate = dacTour3.StartDate ?? DateTime.UtcNow.AddDays(10),
                            EndDate = dacTour3.EndDate ?? DateTime.UtcNow.AddDays(20),
                            Status = "Accepted", // Jockey accepted the invitation
                            InvitationExpiredAt = DateTime.UtcNow.AddDays(2),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.JockeyContracts.Add(contract);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"JockeyContract invitation from Owner-3 to '{jockeyUsername}' for horse '{horseName}' in '{dacTour3Name}' ACCEPTED successfully.");
                    }
                    else if (contract.Status != "Accepted")
                    {
                        contract.Status = "Accepted";
                        _context.JockeyContracts.Update(contract);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Seed Tournament Registration
                    var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.TournamentId == dacTour3.TournamentId && r.HorseId == horse.HorseId);
                    if (registration == null)
                    {
                        registration = new Registration
                        {
                            TournamentId = dacTour3.TournamentId,
                            HorseId = horse.HorseId,
                            Status = "Pending", // Waiting for referee/admin approval
                            RegisteredAt = DateTime.UtcNow
                        };
                        _context.Registrations.Add(registration);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Registration for horse '{horseName}' in '{dacTour3Name}' seeded successfully.");
                    }
                }
            }

            _logger.LogInformation("Mainflow data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding mainflow data.");
            throw;
        }
    }
}
