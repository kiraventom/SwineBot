using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SwineBot.Model;

public class UserContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Swine> Swines { get; set; }
    public DbSet<SwineInfo> Infos { get; set; }
    public DbSet<Feed> Feeds { get; set; }
    public DbSet<WeightLoss> WeightLosses { get; set; }
    public DbSet<DuelRequest> DuelRequests { get; set; }
    public DbSet<DuelResult> DuelResults { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<Slaughter> Slaughters { get; set; }

    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies(false)
            .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            .EnableSensitiveDataLogging(true)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(u =>
        {
            u.HasIndex(u => u.TelegramId).IsUnique();
            u.HasOne<Swine>().WithOne().HasForeignKey<User>(u => u.PrivateSwineId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Group>().HasIndex(g => g.TelegramId).IsUnique();

        modelBuilder.Entity<Swine>(s =>
        {
            s.HasOne<User>().WithMany().HasForeignKey(s => s.OwnerId).OnDelete(DeleteBehavior.Cascade);
            s.HasOne<Group>().WithMany().HasForeignKey(s => s.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SwineInfo>().HasOne<Swine>().WithOne().HasForeignKey<SwineInfo>(i => i.SwineId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Feed>().HasOne<Swine>().WithMany().HasForeignKey(i => i.SwineId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<WeightLoss>().HasOne<Swine>().WithMany().HasForeignKey(i => i.SwineId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DuelRequest>(s =>
        {
            s.HasOne<Swine>().WithMany().HasForeignKey(i => i.AttackerId).OnDelete(DeleteBehavior.Cascade);
            s.HasOne<Swine>().WithMany().HasForeignKey(i => i.DefenderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DuelResult>(s =>
        {
            s.HasOne<Swine>().WithMany().HasForeignKey(i => i.AttackerId).OnDelete(DeleteBehavior.SetNull);
            s.HasOne<Swine>().WithMany().HasForeignKey(i => i.DefenderId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Achievement>(s =>
        {
            s.HasOne<SwineInfo>().WithMany().HasForeignKey(i => i.SwineInfoId).OnDelete(DeleteBehavior.Cascade);
            s.Property(e => e.Type).HasConversion<int>();
        });

        modelBuilder.Entity<Slaughter>(s =>
        {
            s.HasOne<User>().WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            s.HasOne<Group>().WithMany().HasForeignKey(i => i.GroupId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class Group
{
    [Key] public int GroupId { get; set; }

    public long TelegramId { get; set; }

    public string Title { get; set; }
}

public class User
{
    private const double GROWTH_MOD_MULT = 0.0002;

    [Key] public int UserId { get; set; }

    public long TelegramId { get; set; }
    [Required] public string FirstName { get; set; }
    public string Tag { get; set; }

    public int? PrivateSwineId { get; set; }

    public static double GetGrowthModifier(double totalWeightSlaughtered) => Math.Round(1 + (totalWeightSlaughtered * GROWTH_MOD_MULT), 2);
}

public class Swine
{
    [Key] public int SwineId { get; set; }
    public int OwnerId { get; set; }
    public int? GroupId { get; set; }

    [Required] public string Name { get; set; }
    public int Weight { get; set; } // kg
}

public class SwineInfo
{
    [Key] public int InfoId { get; set; }
    public int SwineId { get; set; }
}

public class Feed
{
    [Key] public int FeedId { get; set; }
    public int SwineId { get; set; }

    public DateTime DateTime { get; set; }
    public int Amount { get; set; } // kg
    public double? Luck { get; set; }
}

public class WeightLoss
{
    [Key] public int LossId { get; set; }
    public int SwineId { get; set; }

    public DateTime DateTime { get; set; }
    public int Amount { get; set; } // kg
    public double? Luck { get; set; }

    public bool IsThrowUp { get; set; }
}

public class DuelRequest
{
    [Key] public int RequestId { get; set; }
    public int AttackerId { get; set; }
    public int DefenderId { get; set; }

    public DateTime DateTime { get; set; }
}

public class DuelResult
{
    [Key] public int DuelResultId { get; set; }

    public int? AttackerId { get; set; }
    public int? DefenderId { get; set; }
    public bool AttackerWon { get; set; }

    public DateTime DateTime { get; set; }

    public int WinnerWeightBefore { get; set; }
    public int WinnerWeightAfter { get; set; }
    public int LoserWeightBefore { get; set; }
    public int LoserWeightAfter { get; set; }
}

public enum AchievementType
{
    None = 0,
    Weight = 0x1,
    WeightGain = 0x2,
    WeightLoss = 0x4,
    Overfeed = 0x8,
    NoOverfeed = 0x10,
}

public class Achievement
{
    [Key] public int AchievementId { get; set; }

    public int SwineInfoId { get; set; }

    public DateTime DateTime { get; set; }

    public AchievementType Type { get; set; }
    public int Value { get; set; }
}

public class Slaughter
{
    [Key]
    public int SlaughterId { get; set; }

    public int UserId { get; set; }
    public int? GroupId { get; set; }

    public string SwineName { get; set; }
    public int SwineWeight { get; set; }
    public DateTime DateTime { get; set; }
}
