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
            .Verbatim("\"! 🏆")
            .LineBreak().LineBreak()
            .Italic(viewModel.Level.Description);

        if (viewModel.Level.Effect != null)
        {
            Text
                .LineBreak().LineBreak()
                .Verbatim("✨ Новый эффект: ")
                .Italic(viewModel.Level.Effect.Description)
                .Verbatim(" ✨");
        }
    }
}

