using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements;

public class WeightAchievementChecker(IReadOnlyCollection<AchievementLevel> levels) : AchievementChecker(levels)
{
    protected override AchievementType AchievementType => AchievementType.Weight;

    protected override CheckerResult CheckLevel(BotMessage botMessage, Swine swine, int levelValue)
    {
        if (botMessage is not FeedMessage feedMessage)
            return CheckerResult.Break;

        if (swine.Weight >= levelValue) // Checking if level applies
        {
            if (swine.Weight == feedMessage.NewWeight) // Reacting only to last feed
            {
                return CheckerResult.Apply;
            }

            return CheckerResult.ApplySilent; // levelValue is not too high, but weight was not achieved in last feed
        }

        return CheckerResult.Continue; // levelValue too high
    }
}

