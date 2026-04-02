using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class SetPrivateSwineMessage(ILogger<SetPrivateSwineMessage> logger, int userId) : BotMessage(logger), ISettableSwineId, IPinnableMessage, IStaticMessage
{
    void ISettableSwineId.SetSwineId(int swineId) => SwineIdToSet = swineId;

    public bool ShouldPin => true;

    private int? SwineIdToSet { get; set; }

    protected override Task InitInternal(UserContext userContext, int swineId)
    {
        if (SwineIdToSet == null)
            throw new NotSupportedException($"Can't set selected swine of [{userId}]: {nameof(SwineIdToSet)} is null");

        if (!IsPrivate)
            throw new NotSupportedException($"Can't set selected swine of [{userId}] to [{SwineIdToSet}]: message not private");

        var swineToSet = userContext.Swines.FirstOrDefault(s => s.SwineId == SwineIdToSet);
        if (swineToSet is null)
            throw new NotSupportedException($"Can't set selected swine of [{userId}] to [{SwineIdToSet}]: no such swine in db");

        if (swineToSet.OwnerId != userId)
            throw new NotSupportedException($"Can't set selected swine of [{userId}] to [{SwineIdToSet}]: it belongs to [{swineToSet.OwnerId}]");

        swineId = SwineIdToSet.Value;

        userContext.Users.First(u => u.UserId == userId).PrivateSwineId = SwineIdToSet.Value;
        userContext.SaveChanges();

        var groupTitle = userContext.Groups.First(g => g.GroupId == swineToSet.GroupId).Title;
        Text.Verbatim("Выбранный свин: ").Bold(swineToSet.Name).Verbatim($" из \"{groupTitle}\"");

        return Task.CompletedTask;
    }
}

