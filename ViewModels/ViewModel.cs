using SwineBot.Achievements;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.ViewModels;

public abstract record ViewModel
{
}

public record TopViewModel(IEnumerable<Swine> TopSwines, Swine SenderSwine, int SenderIndex) : ViewModel;

public record SlaughterViewModel(Swine SenderSwine, bool IsTooEarlySlaughter, int AchievsCount, bool ReceivedConfirmation, bool? WasSlaughterEffective) : ViewModel;

public record SetNameViewModel(bool IsNameProvided, bool IsNameTheSame, string NewName) : ViewModel;

public record InfoViewModel(IReadOnlyList<DateTime> RecentFeedDTs, IReadOnlyList<DateTime> RecentThrowupDTs, DateTime UtcNow, User Sender, Swine SenderSwine, int ConsecutiveOverfeeds, int ConsecutiveNoOverfeeds, int WonDuels, int LostDuels, int SlaughtersCount, int GrowthPercent) : ViewModel;

public record HistoryViewModel(byte[] PlotBytes) : ViewModel;

public record WeightChange(DateTime DateTime, int Amount);

public record SwineWeightChanges(string Name, IReadOnlyCollection<WeightChange> WeightChanges);

public record AchievsViewModel(string SwineName, IReadOnlyCollection<DatedAchievementLevel> AchievementLevels) : ViewModel;

public record DatedAchievementLevel(DateTime DT, AchievementLevel Level);

public record AchievementViewModel(string SwineName, AchievementLevel Level) : ViewModel;

public record PiggeryViewModel(int SwinesCount, IEnumerable<SwineFromGroup> SwinesFromGroups, Swine SelectedSwine, string SelectedSwineGroupTitle, bool IsPrivate, string BotUsername) : ViewModel;

public record SwineFromGroup(Swine Swine, string GroupTitle, string SelectStartLink);

public record SetPrivateSwineViewModel : ViewModel, ISettableSwineId
{
    public string SwineName { get; init; }
    public string GroupTitle { get; init; }

    public int SwineId { get; private set; }
    public void SetSwineId(int swineId) => SwineId = swineId;
}

public record StartViewModel(IEnumerable<CommandInfo> CommandInfos) : ViewModel;

public record InvalidViewModel() : ViewModel;

public record FeedViewModel(FeedResult Result, string SwineName, int ConsecutiveOverfeeds) : ViewModel;
