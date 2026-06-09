using HorseRacing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HorseRacing.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser>           Users        { get; set; }
    public DbSet<Role>              Roles        { get; set; }
    public DbSet<JockeyProfile>     JockeyProfiles { get; set; }
    public DbSet<RefereeProfile>    RefereeProfiles { get; set; }
    public DbSet<Horse>             Horses       { get; set; }
    public DbSet<Tournament>        Tournaments  { get; set; }
    public DbSet<Race>              Races        { get; set; }
    public DbSet<RaceEntry>         RaceEntries  { get; set; }
    public DbSet<RaceResult>        RaceResults  { get; set; }
    public DbSet<RaceViolation>     Violations   { get; set; }
    public DbSet<Wallet>            Wallets      { get; set; }
    public DbSet<WalletTransaction> Transactions { get; set; }
    public DbSet<Prediction>        Predictions  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.RoleId);
        });

        modelBuilder.Entity<JockeyProfile>(entity =>
        {
            entity.HasKey(jp => jp.JockeyId);
            entity.HasOne(jp => jp.User)
                .WithOne()
                .HasForeignKey<JockeyProfile>(jp => jp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefereeProfile>(entity =>
        {
            entity.HasKey(rp => rp.RefereeId);
            entity.HasOne(rp => rp.User)
                .WithOne()
                .HasForeignKey<RefereeProfile>(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.WalletId);
            entity.HasOne(w => w.User)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
