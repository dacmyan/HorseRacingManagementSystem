using HorseRacing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser>           Users        { get; set; }
    public DbSet<Horse>             Horses       { get; set; }
    public DbSet<Tournament>        Tournaments  { get; set; }
    public DbSet<Race>              Races        { get; set; }
    public DbSet<RaceEntry>         RaceEntries  { get; set; }
    public DbSet<RaceResult>        RaceResults  { get; set; }
    public DbSet<RaceViolation>     Violations   { get; set; }
    public DbSet<Wallet>            Wallets      { get; set; }
    public DbSet<WalletTransaction> Transactions { get; set; }
    public DbSet<Prediction> Predictions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<RaceEntry>(entity =>
        {
            entity.HasOne(re => re.Jockey)
                .WithMany()
                .HasForeignKey(re => re.JockeyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(re => re.Horse)
                .WithMany()
                .HasForeignKey(re => re.HorseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.SeedData();

        base.OnModelCreating(modelBuilder);
    }
}
