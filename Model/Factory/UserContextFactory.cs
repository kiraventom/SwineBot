using Microsoft.EntityFrameworkCore;

namespace SwineBot.Model.Factory;

public class UserContextFactory : DesignTimeContextFactory<UserContext>
{
    public override UserContext CreateDbContext(string[] args)
    {
        var config = LoadConfig(args);
        var builder = new DbContextOptionsBuilder<UserContext>();
        builder.UseSqlite(config.UserConnectionString);

        return new UserContext(builder.Options);
    }
}
