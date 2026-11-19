using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

/// <summary>
/// Заменяет сообщение вызова, когда защитник отказался или вызов истёк.
/// </summary>
public class DuelResponseMessage : BotMessage<DuelResponseViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelResponseViewModel viewModel)
    {
        switch (viewModel.Status)
        {
            case DuelResponseStatus.Declined:
                Text.Verbatim("\U0001F3F3\ufe0f ")
                    .Bold(viewModel.DefenderSwineName)
                    .Verbatim(" отказывается от вызова ")
                    .Bold(viewModel.AttackerSwineName)
                    .LineBreak()
                    .LineBreak();

                Text.Verbatim(viewModel.DebuffText ?? "Свиной бог промолчал");
                break;

            case DuelResponseStatus.Expired:
                Text.Verbatim("\u231b\ufe0f Вызов от ")
                    .Bold(viewModel.AttackerSwineName)
                    .Verbatim(" свину ")
                    .Bold(viewModel.DefenderSwineName)
                    .Verbatim(" истёк — это засчитано как отказ");

                Text.LineBreak().Verbatim(viewModel.DebuffText ?? "Свиной бог промолчал");
                break;

            default:
                throw new NotSupportedException($"Unexpected duel response status \"{viewModel.Status}\"");
        }
    }
}
