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

    private void WriteAchievement(AchievementLevelViewModel levelViewModel, int index)
    {
        Text.Verbatim(index + 1).Verbatim(". ").Bold(levelViewModel.Level.Name).LineBreak();
        Text.Tab(text =>
        {
            Text.Italic(levelViewModel.Level.Description).LineBreak();
            Text.Verbatim("Получено ").Monospace(levelViewModel.DT.ToString("d MMMM yyyy", Common.RuCulture));

            if (levelViewModel.IsArchived)
                Text.Verbatim(" (архивное)");

            Text.LineBreak();

            if (levelViewModel.LevelsCount > 1)
            {
                if (levelViewModel.LevelIndex == levelViewModel.LevelsCount - 1)
                    Text.Verbatim("Уровень: ").Monospace("Максимальный!").LineBreak();
                else 
                    Text.Verbatim("Уровень: ").Monospace(levelViewModel.LevelIndex + 1).Monospace(" из ").Monospace(levelViewModel.LevelsCount).LineBreak();
            }

            if (levelViewModel.Level.Effect != null)
                Text.Verbatim("Эффект: ").Italic(levelViewModel.Level.Effect.Description).LineBreak();

            Text.LineBreak();
        });
    }
}
