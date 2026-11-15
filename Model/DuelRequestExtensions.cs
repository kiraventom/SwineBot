namespace SwineBot.Model;

public static class DuelRequestExtensions
{
    /// <summary>
    /// How long an unanswered duel request lives. There is no background job that
    /// sweeps requests: every place that reads duel requests drops the expired ones.
    /// </summary>
    public static readonly TimeSpan TTL = TimeSpan.FromHours(24);

    public static bool IsExpired(this DuelRequest request, DateTime utcNow) => utcNow - request.DateTime >= TTL;
}
