using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class AchievementsMessage : BotMessage<AchievsViewModel>
{
    public override void Init<T>(ILogger<T> logger, AchievsViewModel viewModel)
    {
        if (viewModel.AchievementLevels.Count == 0)
        {
            Text.Italic("У ").Bold(viewModel.SwineName).Italic(" пока нет достижений :(").LineBreak();
            return;
        }

        Text.Bold("Достижения ").Bold(viewModel.SwineName).Bold(":").LineBreak().LineBreak();

        int index = 0;
        foreach (var datedLevel in viewModel.AchievementLevels)
        {
            WriteAchievement(datedLevel, index);
            ++index;
        }
    }

    private void WriteAchievement(DatedAchievementLevel datedLevel, int index)
    {
        Text.Verbatim(index + 1).Verbatim(". ").Bold(datedLevel.Level.Name).LineBreak();
        Text.Tab(text =>
        {
            Text.Italic(datedLevel.Level.Description).LineBreak();
            Text.Verbatim("Получено ").Monospace(datedLevel.DT.ToString("d MMMM yyyy", Common.RuCulture)).LineBreak();

            if (datedLevel.Level.Effect != null)
                Text.Verbatim("Эффект: ").Italic(datedLevel.Level.Effect.Description).LineBreak();

            Text.LineBreak();
        });
    }
}
