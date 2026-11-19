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
    public DbSet<SlaughterBonus> SlaughterBonuses { get; set; }

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

        modelBuilder.Entity<SlaughterBonus>(s =>
        {
            s.HasOne<User>().WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            s.HasOne<Group>().WithMany().HasForeignKey(i => i.GroupId).OnDelete(DeleteBehavior.Cascade);
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
    [Key] public int UserId { get; set; }

    public long TelegramId { get; set; }
    [Required] public string FirstName { get; set; }
    public string Tag { get; set; }

    public int? PrivateSwineId { get; set; }

    public static double GetGrowthModifier(double totalWeightSlaughtered)
    {
        var bonus = GrowthBalance.SubLinear
            ? GrowthBalance.PerSqrtKg * Math.Sqrt(Math.Max(0, totalWeightSlaughtered))
            : GrowthBalance.PerKg * totalWeightSlaughtered;

        return Math.Round(1 + bonus, 2);
    }
}

public class Swine
{
    [Key] public int SwineId { get; set; }
    public int OwnerId { get; set; }
    public int? GroupId { get; set; }

    [Required] public string Name { get; set; }
    public int Weight { get; set; } // kg

    /// <summary>Сколько раз свин отказался от дуэли: ценность для забоя падает вдвое за отказ.</summary>
    public int Declines { get; set; }

    /// <summary>Очки славы: начисляются за принятые вызовы, тратятся на отказ.</summary>
    public int Glory { get; set; }

    /// <summary>Когда отвага последний раз затухала на единицу.</summary>
    public DateTime? GloryDecayedAt { get; set; }

    /// <summary>До какого времени корм этого свина может съесть вызывавший.</summary>
    public DateTime? FeedStealUntil { get; set; }

    /// <summary>Кому именно достаётся корм (свин, которого этот свин вызвал или который его вызвал).</summary>
    public int? FeedStealSwineId { get; set; }
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

    public int? MessageId { get; set; }

    public DateTime DateTime { get; set; }
}

public class DuelResult
{
    [Key] public int DuelResultId { get; set; }

    public int? AttackerId { get; set; }
    public int? DefenderId { get; set; }

    /// <summary>Беседа, в которой была дуэль: по ней считается лимит «вызов на беседу».</summary>
    public int? GroupId { get; set; }

    public bool AttackerWon { get; set; }

    /// <summary>
    /// Дуэль не состоялась: защитник отказался и заплатил штраф. Такая запись
    /// даёт кулдаун обеим сторонам, но не считается победой/поражением в статистике.
    /// </summary>
    public bool IsDecline { get; set; }

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
    LowLuck = 0x20,
    HighLuck = 0x40,
}

public class Achievement
{
    [Key] public int AchievementId { get; set; }

    public int SwineInfoId { get; set; }

    public DateTime DateTime { get; set; }

    public AchievementType Type { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Поправка к slaughter-бонусу: дуэль может передать часть бонуса от одного свина другому.
/// Сумма Slaughters.SwineWeight и этих поправок даёт вес, из которого считается
/// множитель кормления (см. User.GetGrowthModifier).
/// </summary>
public class SlaughterBonus
{
    [Key] public int SlaughterBonusId { get; set; }

    public int UserId { get; set; }
    public int GroupId { get; set; }

    /// <summary>Может быть отрицательным: столько бонуса свин отдал.</summary>
    public int Weight { get; set; }

    public DateTime DateTime { get; set; }
    public string Reason { get; set; }
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
