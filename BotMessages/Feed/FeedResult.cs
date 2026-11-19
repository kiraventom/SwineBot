namespace SwineBot.BotMessages.Feed;

public enum Result { FirstFeed, Overfeed, Throwup, Full }

public record FeedResult
{
    public int Amount { get; init; }

    /// <summary>Порция без множителей: из неё видно, сколько массы дал именно бонус.</summary>
    public int BaseAmount { get; init; }

    public int OldWeight { get; init; }
    public DateTime UtcDT { get; init; }
    public double Luck { get; init; }
    public int RecentFeedsCount { get; init; }
    public Result Result { get; init; }

    public int NewWeight => OldWeight + Amount;
    public int AbsAmount => Math.Abs(Amount);

    public static FeedResult Full { get; } = new FeedResult() { Result = Result.Full };
}

