using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    public User GetOrAddUser(long chatId, string title, long senderId, string firstName, string username)
    {
        var user = this.Users.FirstOrDefault(u => u.TelegramId == senderId);

        bool newUser = user is null;
        if (newUser)
        {
            user = new User()
            {
                FirstName = firstName,
                Tag = username,
                TelegramId = senderId,
            };

            Users.Add(user);
            SaveChanges();
        }

        var group = this.Groups.FirstOrDefault(g => g.TelegramId == chatId);
        bool newGroup = group is null;

        if (newGroup)
        {
            group = new Group()
            {
                Title = title,
                TelegramId = chatId,
            };

            Groups.Add(group);
            SaveChanges();
        }

        if (newUser || newGroup)
        {
            var swine = new Swine()
            {
                Name = firstName,
                Weight = 1,
                OwnerId = user.UserId,
                GroupId = group.GroupId
            };

            Swines.Add(swine);
            SaveChanges();

            var info = new SwineInfo()
            {
                SwineId = swine.SwineId
            };

            Infos.Add(info);
            SaveChanges();
        }

        if (user.FirstName != firstName)
            user.FirstName = firstName;

        if (user.Tag != username)
            user.Tag = username;

        if (group.Title != title)
            group.Title = title;

        SaveChanges();

        return user;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies(false);
        optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>().Property(e => e.Type).HasConversion<int>();
    }
}

[Index(nameof(TelegramId), IsUnique = true)]
public class Group
{
    [Key] public int GroupId { get; set; }

    public long TelegramId { get; set; }

    public string Title { get; set; }
}

[Index(nameof(TelegramId), IsUnique = true)]
public class User
{
    private const double GROWTH_MOD_MULT = 0.0002;

    [Key] public int UserId { get; set; }

    public long TelegramId { get; set; }
    [Required] public string FirstName { get; set; }
    public string Tag { get; set; }

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
}

public class WeightLoss
{
    [Key] public int LossId { get; set; }
    public int SwineId { get; set; }

    public DateTime DateTime { get; set; }
    public int Amount { get; set; } // kg

    public bool IsThrowUp { get; set; }

    [NotMapped]
    public bool Ignored => Amount == 0;
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

    public int AttackerId { get; set; }
    public int DefenderId { get; set; }
    public bool AttackerWon { get; set; }

    [NotMapped]
    public int WinnerId => AttackerWon ? AttackerId : DefenderId;

    [NotMapped]
    public int LoserId => AttackerWon ? DefenderId : AttackerId;

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
