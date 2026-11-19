using Microsoft.Extensions.Logging;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

/// <summary>
/// Заменяет сообщение вызова итогом состоявшегося боя.
/// </summary>
public class DuelResultMessage : BotMessage<DuelResultViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, DuelResultViewModel viewModel)
    {
        Text.Verbatim("\u2694\ufe0f ")
            .Bold(viewModel.WinnerSwineName)
            .Verbatim(" побеждает ")
            .Bold(viewModel.LoserSwineName)
            .LineBreak()
            .LineBreak();

        Text.Bold(viewModel.WinnerSwineName)
            .Verbatim($": {viewModel.WinnerWeightBefore} \u2192 {viewModel.WinnerWeightAfter} кг ")
            .Monospace($"(+{viewModel.StakeKg})")
            .LineBreak();

        Text.Bold(viewModel.LoserSwineName)
            .Verbatim($": {viewModel.LoserWeightBefore} \u2192 {viewModel.LoserWeightAfter} кг ")
            .Monospace($"(\u2212{viewModel.StakeKg})");
    }
}
