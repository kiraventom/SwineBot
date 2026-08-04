using Microsoft.Extensions.Logging;
using SwineBot.Achievements;
using SwineBot.Text;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class TooSoonToDuelMessage : BotMessage<TooSoonToDuelViewModel>
{
    public override void Init<TMessage>(ILogger<TMessage> logger, TooSoonToDuelViewModel viewModel)
    {
        Text.Verbatim("В памяти ").Bold(viewModel.SwineName).Verbatim(" ещё слишком свежи воспоминания о битве с ").Bold(viewModel.LastOpponentName).LineBreak().LineBreak();
        
        var noun = MessageTextUtils.GetDeclinatedNoun(viewModel.HoursLeft, Unit.Hour);
        Text.Verbatim($"Судя по выражению морды свина, он будет готов к новой дуэли через {viewModel.HoursLeft} {noun}");
    }
}
