using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

/// <summary>
/// Replaces the duel request message after the defender pressed accept/decline
/// (or after the request expired).
/// </summary>
public class DuelResponseMessage : BotMessage<DuelResponseViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelResponseViewModel viewModel)
    {
        switch (viewModel.Status)
        {
            case DuelResponseStatus.Accepted:
                Text.Verbatim("\u2694\ufe0f ")
                    .Bold(viewModel.DefenderSwineName)
                    .Verbatim(" принимает вызов от ")
                    .Bold(viewModel.AttackerSwineName);
                break;

            case DuelResponseStatus.Declined:
                Text.Verbatim("\U0001F3F3\ufe0f ")
                    .Bold(viewModel.DefenderSwineName)
                    .Verbatim(" отклоняет вызов от ")
                    .Bold(viewModel.AttackerSwineName);
                break;

            case DuelResponseStatus.Expired:
                Text.Verbatim("\u231b\ufe0f Вызов от ")
                    .Bold(viewModel.AttackerSwineName)
                    .Verbatim(" свину ")
                    .Bold(viewModel.DefenderSwineName)
                    .Verbatim(" истёк");
                break;

            default:
                throw new NotSupportedException($"Unexpected duel response status \"{viewModel.Status}\"");
        }
    }
}
