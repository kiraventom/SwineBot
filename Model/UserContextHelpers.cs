using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SwineBot.Model;

public record SenderInfo(int UserId, int? GroupId);

public class UserContextHelpers(ILogger<UserContextHelpers> logger, UserContext context)
{
    public async Task<SenderInfo> GetOrAddUser(long chatId, string title, long senderId, string firstName, string username)
    {
        var isPrivate = chatId > 0;

        var user = await context.Users.AsTracking().FirstOrDefaultAsync(u => u.TelegramId == senderId);

        if (user is null)
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

        var senderInfo = new SenderInfo(user.UserId, null);

        if (!isPrivate)
        {
            var group = await context.Groups.AsTracking().FirstOrDefaultAsync(g => g.TelegramId == chatId);
            if (group is null)
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

            senderInfo = senderInfo with { GroupId = group.GroupId };

            bool hasSwine = context.Swines.Any(s => s.OwnerId == user.UserId && s.GroupId == group.GroupId);

            if (!hasSwine)
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

        return senderInfo;
    }

    public async Task<int?> GetOrSetSwine(SenderInfo senderInfo)
    {
        var isPrivate = senderInfo.GroupId == null;

        if (!isPrivate)
        {
            var swineId = (await context.Swines
                .Where(s => s.GroupId == senderInfo.GroupId)
                .FirstAsync(s => s.OwnerId == senderInfo.UserId))
                .SwineId;

            logger.LogInformation("Found swine [{swine}] for user [{user}] in group [{group}]", swineId, senderInfo.UserId, senderInfo.GroupId);
            return swineId;
        }

        var user = await context.Users.AsTracking().FirstAsync(u => u.UserId == senderInfo.UserId);
        var userSwines = context.Swines.Where(s => s.OwnerId == user.UserId);
        var userSwinesCount = await userSwines.CountAsync();
        var privateSwineId = user.PrivateSwineId;

        if (userSwines.All(s => s.SwineId != privateSwineId))
        {
            user.PrivateSwineId = null;

            logger.LogInformation("User [{user}] has private swine [{private}], but his swines are [ {swines} ]. Probably slaughtered swine, set private swine to null", user.UserId, privateSwineId, string.Join(", ", userSwines.Select(s => s.SwineId)));
        }

        // If no private swine is selected, auto-select the swine if it's user's only one
        if (privateSwineId is null)
        {
            if (userSwinesCount == 1)
            {
                user.PrivateSwineId = (await userSwines.FirstAsync()).SwineId;
                privateSwineId = user.PrivateSwineId;

                logger.LogInformation("User [{user}] has no private swine, but his only swine is [{swine}]. Set private swine to [{swine}]", user.UserId, privateSwineId, privateSwineId);
            }
        }

        return privateSwineId;
    }

}

