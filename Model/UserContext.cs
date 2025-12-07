using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwineBot.Model;

public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Swine> Swines { get; set; }
    public DbSet<SwineInfo> Infos { get; set; }
    public DbSet<Feed> Feeds { get; set; }
    public DbSet<WeightLoss> WeightLosses { get; set; }
    public DbSet<DuelRequest> DuelRequests { get; set; }
    public DbSet<DuelResult> DuelResults { get; set; }

    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    public static UserContext Create()
    {
        var builder = new DbContextOptionsBuilder<UserContext>();
        builder.UseSqlite(Config.Instance.UserConnectionString);

        return new UserContext(builder.Options);
    }

    public User GetOrAddUser(long telegramId, string firstName, string username)
    {
        var isNew = false;

        var userModel = this.Users
            .FirstOrDefault(u => u.TelegramId == telegramId);

        if (userModel is null)
        {
            isNew = true;
            userModel = new User()
            {
                FirstName = firstName,
                Tag = username,
                TelegramId = telegramId,
                Swine = new Swine()
                {
                    Name = firstName,
                    Stats = new(),
                    Weight = 1,
                }
            };
        }

        if (userModel.FirstName != firstName)
            userModel.FirstName = firstName;

        if (userModel.Tag != username)
            userModel.Tag = username;

        if (isNew)
        {
            Users.Add(userModel);
            this.SaveChanges();
        }

        return userModel;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}

[Index(nameof(TelegramId), IsUnique=true)]
public class User
{
    [Key] public int UserId { get; set; }

    public long TelegramId { get; set; }
    [Required] public string FirstName { get; set; }
    public string Tag { get; set; }

    [InverseProperty(nameof(Swine.Owner))]
    public Swine Swine { get; set; }
}

public class Swine
{
    [Key] public int SwineId { get; set; }
    public int OwnerId { get; set; }
    public int StatsId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public User Owner { get; set; }

    [InverseProperty(nameof(Stats.Swine))]
    public SwineInfo Stats { get; set; }

    [Required] public string Name { get; set; }

    public int Weight { get; set; } // kg

    [InverseProperty(nameof(Feed.Swine))]
    public List<Feed> Feeds { get; } = new();
}

public class SwineInfo
{
    [Key] public int InfoId { get; set; }
    public int SwineId { get; set; }

    [ForeignKey(nameof(SwineId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public Swine Swine { get; set; }
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

    public int WinnerId => AttackerWon ? AttackerId : DefenderId;
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
