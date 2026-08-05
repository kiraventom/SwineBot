using Microsoft.Extensions.Logging;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class DuelRequestMessage : BotMessage<DuelRequestViewModel>, ICustomRecepientMessage
{
    public Recepient CustomRecepient { get; set; }

    public override void Init<TMessage>(ILogger<TMessage> logger, DuelRequestViewModel viewModel)
    {
        if (viewModel.ReminderMode)
            Text.Underline("Вас вызвали на дуэль!").LineBreak().LineBreak();

        if (string.IsNullOrEmpty(viewModel.Tag))
        {
            Text.InlineMention(viewModel.UserName, viewModel.Id);
        }
        else
        {
            Text.Verbatim(viewModel.Tag);
        }

        Text
            .Verbatim(", вашего свина ")
            .Bold(viewModel.SwineName)
            .Verbatim(" вызывает на дуэль ")
            .Bold(viewModel.CallerSwineName)
            .Verbatim(" под руководством ")
            .Bold(viewModel.CallerUserName)
            .LineBreak()
            .LineBreak();

        var winChanceStr = $"{viewModel.WinChance}%";
        if (viewModel.WinChance <= 0)
            winChanceStr = "ничтожные";
        else if (viewModel.WinChance >= 100)
            winChanceStr = "огромные";

        var penaltyStr = string.Empty;
        if (viewModel.DeclinePenalty < 0)
            penaltyStr = "Бафф за отказ: ";
        else if (viewModel.DeclinePenalty > 0)
            penaltyStr = "Штраф за отказ: ";

        Text.Verbatim("Шанс на победу: ").Monospace(winChanceStr).LineBreak();

        if (viewModel.DeclinePenalty != 0)
            Text.Verbatim(penaltyStr).Monospace(Math.Abs(viewModel.DeclinePenalty)).Monospace("%").LineBreak();

        Text.Italic("Принять вызов?");

        Keyboard.AddButton("✅", $"accept_{viewModel.DuelRequestId}");
        Keyboard.AddButton("❌", $"decline_{viewModel.DuelRequestId}");
    }
}
