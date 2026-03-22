using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

    public static UserContext Create()
    {
        var builder = new DbContextOptionsBuilder<UserContext>();
        builder.UseSqlite(Config.Instance.UserConnectionString);

        return new UserContext(builder.Options);
    }

    public User GetOrAddUser(long chatId, string title, long senderId, string firstName, string username)
    {
        var isNewUser = false;

        var user = this.Users.Include(u => u.Swines).FirstOrDefault(u => u.TelegramId == senderId);

        if (user is null)
        {
            isNewUser = true;
            user = new User()
            {
                FirstName = firstName,
                Tag = username,
                TelegramId = senderId,
                Swines = []
            };
        }

        var isNewGroup = false;
        var group = this.Groups.FirstOrDefault(g => g.TelegramId == chatId);

        if (group is null)
        {
            isNewGroup = true;
            group = new Group()
            {
                Title = title,
                TelegramId = chatId,
                Swines = []
            };

            Log.Logger.Information("User [{userId}] chat [{chatId}]: New group, creating new swine", senderId, chatId);

            var swine = new Swine()
            {
                Name = firstName,
                Info = new(),
                Weight = 1,
                Owner = user,
            };

            group.Swines.Add(swine);
        }

        if (user.FirstName != firstName)
            user.FirstName = firstName;

        if (user.Tag != username)
            user.Tag = username;

        if (group.Title != title)
            group.Title = title;

        if (isNewGroup)
        {
            Groups.Add(group);
        }

        if (isNewUser)
        {
            Users.Add(user);
        }

        this.SaveChanges();

        return user;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>().Property(e => e.Type).HasConversion<int>();
    }
}

[Index(nameof(TelegramId), IsUnique=true)]
public class Group
{
    [Key] public int GroupId { get; set; }
    
    public long TelegramId { get; set; }

    public string Title { get; set; }

    [InverseProperty(nameof(Swine.Group))]
    public List<Swine> Swines { get; set; }
}

[Index(nameof(TelegramId), IsUnique=true)]
public class User
{
    private const double GROWTH_MOD_MULT = 0.0002;

    [Key] public int UserId { get; set; }

    public long TelegramId { get; set; }
    [Required] public string FirstName { get; set; }
    public string Tag { get; set; }

    [InverseProperty(nameof(Swine.Owner))]
    public List<Swine> Swines { get; set; }

    [InverseProperty(nameof(Slaughter.User))]
    public List<Slaughter> Slaughters { get; } = new();

    public static double GetGrowthModifier(double totalWeightSlaughtered) => Math.Round(1 + (totalWeightSlaughtered * GROWTH_MOD_MULT), 2);
}

public class Swine
{
    [Key] public int SwineId { get; set; }
    public int OwnerId { get; set; }
    public int? GroupId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public User Owner { get; set; }

    [ForeignKey(nameof(GroupId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Group Group { get; set; }

    [InverseProperty(nameof(SwineInfo.Swine))]
    public SwineInfo Info { get; set; }

    [Required] public string Name { get; set; }

    public int Weight { get; set; } // kg

    [InverseProperty(nameof(Feed.Swine))]
    public List<Feed> Feeds { get; } = new();

    [InverseProperty(nameof(WeightLoss.Swine))]
    public List<WeightLoss> WeightLosses { get; } = new();
}

public class SwineInfo
{
    [Key] public int InfoId { get; set; }
    public int SwineId { get; set; }

    [ForeignKey(nameof(SwineId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Swine { get; set; }

    [InverseProperty(nameof(Achievement.SwineInfo))]
    public List<Achievement> Achievements { get; } = new();
}

public class Feed
{
    [Key] public int FeedId { get; set; }
    public int SwineId { get; set; }

    [ForeignKey(nameof(SwineId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Swine { get; set; }

    public DateTime DateTime { get; set; }
    public int Amount { get; set; } // kg
}

public class WeightLoss
{
    [Key] public int LossId { get; set; }
    public int SwineId { get; set; }

    [ForeignKey(nameof(SwineId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Swine { get; set; }

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

    [ForeignKey(nameof(AttackerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Attacker { get; set; }

    [ForeignKey(nameof(DefenderId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Defender { get; set; }

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

    [ForeignKey(nameof(AttackerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Attacker { get; set; }

    [ForeignKey(nameof(DefenderId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Defender { get; set; }

    [ForeignKey(nameof(WinnerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Winner { get; set; }

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

    [ForeignKey(nameof(SwineInfoId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public SwineInfo SwineInfo { get; set; }

    public AchievementType Type { get; set; }
    public int Value { get; set; }
}

public class Slaughter
{
    [Key]
    public int SlaughterId { get; set; }

    public int UserId { get; set; }
    public int? GroupId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; }

    public string SwineName { get; set; }
    public int SwineWeight { get; set; }
    public DateTime DateTime { get; set; }
}
