using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages;

public class AchievementMessage : BotMessage<AchievementViewModel>
{
    public override void Init<T>(ILogger<T> logger, AchievementViewModel viewModel)
    {
        Text.Verbatim("🏆")
            .Bold(viewModel.SwineName)
            .Verbatim(" получает достижение \"")
            .Bold(viewModel.Level.Name)
            .Verbatim("\"");

        if (viewModel.LevelsCount > 1)
        {
            if (viewModel.LevelIndex == viewModel.LevelsCount - 1)
                Text.Verbatim(" (макс. уровень)");
            else 
                Text.Verbatim($" (уровень {viewModel.LevelIndex + 1}/{viewModel.LevelsCount})");
        }

        Text.Verbatim("! 🏆")
            .LineBreak().LineBreak()
            .Italic(viewModel.Level.Description).LineBreak();


        if (viewModel.Level.Effect != null)
        {
            Text
                .LineBreak()
                .Verbatim("✨ Новый эффект: ")
                .Italic(viewModel.Level.Effect.Description)
                .Verbatim(" ✨").LineBreak();
        }
    }
}

