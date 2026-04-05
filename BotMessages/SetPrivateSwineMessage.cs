using Microsoft.Extensions.Logging;
using SwineBot.Model;

namespace SwineBot.BotMessages;

public class SetPrivateSwineMessage(ILogger<SetPrivateSwineMessage> logger, UserContext context) : BotMessage(logger), ISettableSwineId, IPinnableMessage, IStaticMessage
{
    void ISettableSwineId.SetSwineId(int swineId) => SwineIdToSet = swineId;

    public bool ShouldPin => true;

    private int? SwineIdToSet { get; set; }

    protected override async Task InitInternal(Update update)
    {
        if (SwineIdToSet == null)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}]: {nameof(SwineIdToSet)} is null");

        if (!update.IsPrivateChat)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{SwineIdToSet}]: message not private");

        var swineToSet = context.Swines.FirstOrDefault(s => s.SwineId == SwineIdToSet);
        if (swineToSet is null)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{SwineIdToSet}]: no such swine in db");

        if (swineToSet.OwnerId != update.UserId)
            throw new NotSupportedException($"Can't set selected swine of [{update.UserId}] to [{SwineIdToSet}]: it belongs to [{swineToSet.OwnerId}]");

        context.Users.First(u => u.UserId == update.UserId).PrivateSwineId = SwineIdToSet.Value;
        await context.SaveChangesAsync();

        var groupTitle = context.Groups.First(g => g.GroupId == swineToSet.GroupId).Title;
        Text.Verbatim("Выбранный свин: ").Bold(swineToSet.Name).Verbatim($" из \"{groupTitle}\"");
    }
}

