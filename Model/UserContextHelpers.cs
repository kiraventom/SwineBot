using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwineBot.Model;

public class UserContextHelpers(ILogger<UserContextHelpers> logger, UserContext context)
{
    public async Task<int> GetOrAddUser(long chatId, string title, long senderId, string firstName, string username)
    {
        var isPrivate = chatId > 0;

        var user = context.Users.FirstOrDefault(u => u.TelegramId == senderId);

        bool newUser = user is null;
        if (newUser)
        {
            user = new User()
            {
                FirstName = firstName,
                Tag = username,
                TelegramId = senderId,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            logger.LogInformation("Created new user [{user}] \"{name}\"", user.UserId, user.FirstName);
        }

        var group = context.Groups.FirstOrDefault(g => g.TelegramId == chatId);
        bool newGroup = group is null;

        if (!isPrivate)
        {
            if (newGroup)
            {
                group = new Group()
                {
                    Title = title,
                    TelegramId = chatId,
                };

                context.Groups.Add(group);
                await context.SaveChangesAsync();

                logger.LogInformation("Created new group [{group}] \"{name}\"", group.GroupId, group.Title);
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

                context.Swines.Add(swine);
                await context.SaveChangesAsync();

                var info = new SwineInfo()
                {
                    SwineId = swine.SwineId
                };

                context.Infos.Add(info);
                await context.SaveChangesAsync();

                logger.LogInformation("Created new swine [{swine}] with info [{info}]", swine.SwineId, info.InfoId);
            }

            if (group.Title != title)
            {
                var oldTitle = group.Title;
                group.Title = title;

                logger.LogInformation("Detected group title changing from \"{old}\" to \"{new}\"", oldTitle, title);
            }
        }

        if (user.FirstName != firstName)
        {
            var oldName = user.FirstName;
            user.FirstName = firstName;

            logger.LogInformation("Detected user first name changing from \"{old}\" to \"{new}\"", oldName, firstName);
        }

        if (user.Tag != username)
        {
            var oldTag = user.Tag;
            user.Tag = username;

            logger.LogInformation("Detected user tag changing from \"{old}\" to \"{new}\"", oldTag, username);
        }

        await context.SaveChangesAsync();

        return user.UserId;;
    }

    public async Task<int?> GetOrSetSwine(int? groupId, int userId)
    {
        var isPrivate = groupId == null;

        if (!isPrivate)
        {
            var swineId = context.Swines
                .Where(s => s.GroupId == groupId)
                .First(s => s.OwnerId == userId)
                .SwineId;

            logger.LogInformation("Found swine [{swine}] for user [{user}] in group [{group}]", swineId, userId, groupId);
            return swineId;
        }

        var user = context.Users.First(u => u.UserId == userId);
        var userSwines = await context.Swines.AsNoTracking().Where(s => s.OwnerId == userId).ToListAsync();
        var privateSwineId = user.PrivateSwineId;

        if (userSwines.All(s => s.SwineId != privateSwineId))
        {
            user.PrivateSwineId = null;
            await context.SaveChangesAsync();

            logger.LogInformation("User [{user}] has private swine [{private}], but his swines are [ {swines} ]. Probably slaughtered swine, set private swine to null", userId, privateSwineId, string.Join(", ", userSwines.Select(s => s.SwineId)));
        }

        // If no private swine is selected, auto-select the swine if it's user's only one
        if (privateSwineId is null)
        {
            if (userSwines.Count == 1)
            {
                user.PrivateSwineId = userSwines.First().SwineId;
                privateSwineId = user.PrivateSwineId;

                await context.SaveChangesAsync();

                logger.LogInformation("User [{user}] has no private swine, but his only swine is [{swine}]. Set private swine to [{swine}]", userId, privateSwineId, privateSwineId);
            }
        }

        return privateSwineId;
    }

}

