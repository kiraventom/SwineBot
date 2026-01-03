using SwineBot.BotMessages;
using SwineBot.Model;

namespace SwineBot.Achievements.Checkers;

public class InvalidAchievementChecker() : AchievementChecker(Array.Empty<AchievementLevel>())
{
    public override AchievementType Type => AchievementType.None;

    protected override bool DoesLevelApply(int value, int level) => false;

    protected override int? GetValue(BotMessage botMessage, Swine swine) => null;

    protected override bool IsSilentApply(BotMessage botMessage, Swine swine) => true;
}

