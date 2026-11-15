using Microsoft.Extensions.Logging;
using SwineBot.Updates;
using SwineBot.ViewModels;

namespace SwineBot.BotMessages.Duels;

public class DuelRequestMessage : BotMessage<DuelRequestViewModel>, ICustomRecepientMessage
{
    public Recepient CustomRecepient { get; set; }

    public const string AcceptCallbackDataPrefix = "accept_";
    public const string DeclineCallbackDataPrefix = "decline_";

    public override void Init<TMessage>(ILogger<TMessage> logger, DuelRequestViewModel viewModel)
    {
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

        Text.Verbatim("Шанс на победу: ").Monospace(winChanceStr).LineBreak();

        Text.Verbatim("Ставка: ").Monospace($"{viewModel.StakePercent}%")
            .Verbatim(" веса проигравшего, для вас это ")
            .Monospace($"{viewModel.StakeKg} кг")
            .LineBreak();

        Text.Verbatim("Отказ: ").Monospace($"\u2212{viewModel.DeclineBonusKg}")
            .Verbatim(" бонуса забоев в пользу вызывающего")
            .LineBreak();

        if (viewModel.DefenderKills > 0 || viewModel.AttackerKills > 0)
        {
            Text.Italic("Забитых предшественников: ")
                .Verbatim($"{viewModel.DefenderKills} у вас, {viewModel.AttackerKills} у вызывающего")
                .Verbatim(" — каждый режет шанс на ")
                .Monospace("2%")
                .LineBreak();
        }

        Text.LineBreak();

        Text.Italic("Принять вызов?");

        Keyboard.AddButton("\u2705", $"{AcceptCallbackDataPrefix}{viewModel.DuelRequestId}");
        Keyboard.AddButton("\u274c", $"{DeclineCallbackDataPrefix}{viewModel.DuelRequestId}");
    }
}
