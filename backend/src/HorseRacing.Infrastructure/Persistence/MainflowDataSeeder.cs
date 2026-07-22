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
        _logger.LogInformation("Starting Mainflow data seeding...");

        try
        {
            _logger.LogInformation("Mainflow data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding mainflow data.");
            throw;
        }
    }
}
