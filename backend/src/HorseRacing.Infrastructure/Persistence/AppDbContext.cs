using HorseRacing.Domain.Entities;
using HorseRacing.Domain.Entities.Tournaments;
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
    public DbSet<Round>             Rounds       { get; set; }
    public DbSet<Race>              Races        { get; set; }
    public DbSet<RaceEntry>         RaceEntries  { get; set; }
    public DbSet<Registration>      Registrations { get; set; }
    public DbSet<JockeyContract>    JockeyContracts { get; set; }
    public DbSet<RaceResult>        RaceResults  { get; set; }
    public DbSet<RaceRefereeAssignment> RaceRefereeAssignments { get; set; }
    public DbSet<RaceViolation>     Violations   { get; set; }
    public DbSet<Wallet>            Wallets      { get; set; }
    public DbSet<WalletTransaction> Transactions { get; set; }
    public DbSet<Prediction>        Predictions  { get; set; }
    public DbSet<Bet>               Bets         { get; set; }
    public DbSet<Payout>            Payouts      { get; set; }
    public DbSet<Prize>             Prizes       { get; set; }
    public DbSet<TournamentPrizePayout> TournamentPrizePayouts { get; set; }
    public DbSet<Notification>      Notifications { get; set; }
    public DbSet<HorseDocument>     HorseDocuments { get; set; }
    public DbSet<HorseStatistic>    HorseStatistics { get; set; }
    public DbSet<RefereeReport>     RefereeReports { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");
            entity.HasKey(u => u.UserId);
            entity.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(r => r.RoleId);
        });

        modelBuilder.Entity<JockeyProfile>(entity =>
        {
            entity.ToTable("JockeyProfile");
            entity.HasKey(jp => jp.JockeyId);
            entity.HasOne(jp => jp.User)
                .WithOne()
                .HasForeignKey<JockeyProfile>(jp => jp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefereeProfile>(entity =>
        {
            entity.ToTable("RefereeProfile");
            entity.HasKey(rp => rp.RefereeId);
            entity.HasOne(rp => rp.User)
                .WithOne()
                .HasForeignKey<RefereeProfile>(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("Wallet");
            entity.HasKey(w => w.WalletId);
            entity.HasOne(w => w.User)
                .WithOne()
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<RaceEntry>(entity =>
        {
            entity.ToTable("RaceEntry");
            entity.HasKey(re => re.RaceEntryId);

            entity.HasOne(re => re.Race)
                .WithMany()
                .HasForeignKey(re => re.RaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(re => re.Registration)
                .WithMany()
                .HasForeignKey(re => re.RegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(re => re.Jockey)
                .WithMany()
                .HasForeignKey(re => re.JockeyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(re => re.WinningProbability)
                .HasPrecision(5, 2);

            entity.Property(re => re.CurrentOdds)
                .HasPrecision(10, 2);

            entity.HasIndex(re => new { re.RaceId, re.LaneNo })
                .IsUnique();

            entity.HasIndex(re => new { re.RaceId, re.RegistrationId })
                .IsUnique();
        });

        modelBuilder.Entity<JockeyContract>(entity =>
        {
            entity.ToTable("JockeyContract");
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Horse)
                .WithMany()
                .HasForeignKey(c => c.HorseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Jockey)
                .WithMany()
                .HasForeignKey(c => c.JockeyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registration");
            entity.HasKey(r => r.RegistrationId);

            entity.HasOne(r => r.Tournament)
                .WithMany()
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Horse)
                .WithMany(h => h.Registrations)
                .HasForeignKey(r => r.HorseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.TournamentId, r.HorseId })
                .IsUnique();
        });

        modelBuilder.Entity<Bet>(entity =>
        {
            entity.ToTable("Bet");
            entity.HasKey(b => b.Id);
            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(b => b.Race)
                .WithMany()
                .HasForeignKey(b => b.RaceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(b => b.Horse)
                .WithMany()
                .HasForeignKey(b => b.HorseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.ToTable("Payout");
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.Bet)
                .WithMany()
                .HasForeignKey(p => p.BetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Prize>(entity =>
        {
            entity.ToTable("Prize");
            entity.HasKey(pr => pr.Id);
            entity.HasOne(pr => pr.Tournament)
                .WithMany()
                .HasForeignKey(pr => pr.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TournamentPrizePayout>(entity =>
        {
            entity.ToTable("TournamentPrizePayout");
            entity.HasKey(tpp => tpp.Id);
            entity.HasOne(tpp => tpp.Tournament)
                .WithMany()
                .HasForeignKey(tpp => tpp.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(tpp => tpp.User)
                .WithMany()
                .HasForeignKey(tpp => tpp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");
            entity.HasKey(n => n.Id);
            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HorseDocument>(entity =>
        {
            entity.ToTable("HorseDocument");
            entity.HasKey(hd => hd.Id);
            entity.HasOne(hd => hd.Horse)
                .WithMany(h => h.Documents)
                .HasForeignKey(hd => hd.HorseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HorseStatistic>(entity =>
        {
            entity.ToTable("HorseStatistic");
            entity.HasKey(hs => hs.Id);
            entity.HasOne(hs => hs.Horse)
                .WithOne(h => h.Statistic)
                .HasForeignKey<HorseStatistic>(hs => hs.HorseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RaceRefereeAssignment>(entity =>
        {
            entity.ToTable("RaceRefereeAssignment");
            entity.HasKey(rra => rra.AssignmentId);
            entity.HasOne(rra => rra.Race)
                .WithMany(r => r.RaceRefereeAssignments)
                .HasForeignKey(rra => rra.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rra => rra.RefereeProfile)
                .WithMany()
                .HasForeignKey(rra => rra.RefereeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.ToTable("Tournament");
            entity.HasKey(t => t.TournamentId);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.ToTable("Round");
            entity.HasKey(r => r.RoundId);
            entity.HasOne(r => r.Tournament)
                .WithMany(t => t.Rounds)
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.ToTable("Race");
            entity.HasKey(r => r.RaceId);
            entity.HasOne(r => r.Round)
                .WithMany(round => round.Races)
                .HasForeignKey(r => r.RoundId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Horse>(entity =>
        {
            entity.ToTable("Horse");
            entity.HasKey(h => h.HorseId);
        });
        modelBuilder.Entity<RaceResult>().ToTable("RaceResult");
        modelBuilder.Entity<Prediction>().ToTable("Prediction");
        modelBuilder.Entity<WalletTransaction>().ToTable("WalletTransaction");
        modelBuilder.Entity<RaceViolation>().ToTable("RaceViolation");

        modelBuilder.Entity<RefereeReport>(entity =>
        {
            entity.ToTable("RefereeReport");
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Race)
                .WithMany()
                .HasForeignKey(r => r.RaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.RefereeProfile)
                .WithMany()
                .HasForeignKey(r => r.RefereeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReportedHorse)
                .WithMany()
                .HasForeignKey(r => r.ReportedHorseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.SeedData();

        base.OnModelCreating(modelBuilder);
    }
}
