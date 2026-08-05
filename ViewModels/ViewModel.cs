using SwineBot.Achievements;
using SwineBot.Actions.Commands;
using SwineBot.BotMessages;
using SwineBot.BotMessages.Feed;
using SwineBot.Model;

namespace SwineBot.ViewModels;

public abstract record ViewModel;

public record TopViewModel(IEnumerable<Swine> TopSwines, Swine SenderSwine, int SenderIndex) : ViewModel;

public record SlaughterViewModel(Swine SenderSwine, bool IsTooEarlySlaughter, int AchievsCount, bool ReceivedConfirmation, bool? WasSlaughterEffective) : ViewModel;

public record SetNameViewModel(bool IsNameProvided, bool IsNameTheSame, string NewName) : ViewModel;

public record InfoViewModel(IReadOnlyList<DateTime> RecentFeedDTs, IReadOnlyList<DateTime> RecentThrowupDTs, DateTime UtcNow, User Sender, Swine SenderSwine, int ConsecutiveOverfeeds, int ConsecutiveNoOverfeeds, int WonDuels, int LostDuels, int SlaughtersCount, int GrowthPercent, string OutcomingDuelTargetName, string IncomingDuelSourceName) : ViewModel;

public record HistoryViewModel(byte[] PlotBytes) : ViewModel;

public record WeightChange(DateTime DateTime, int Amount);

public record SwineWeightChanges(string Name, IReadOnlyCollection<WeightChange> WeightChanges);

public record AchievsViewModel(string SwineName, IReadOnlyCollection<AchievementLevelViewModel> AchievementLevels) : ViewModel;

public record AchievementLevelViewModel(DateTime DT, AchievementLevel Level, int LevelIndex, int LevelsCount, bool IsArchived);

public record AchievementViewModel(string SwineName, AchievementLevel Level, int LevelIndex, int LevelsCount) : ViewModel;

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

public record FeedViewModel(FeedResult Result, string SwineName, int ConsecutiveOverfeeds) : ViewModel
{
    public int HoursLeftUntilCanEat { get; init; }
}

public record SelectDuelOpponentViewModel(string Query, string GroupName, int OpponentsCount, string CurrentRequestOpponent) : ViewModel;

public record DuelRequestViewModel(int DuelRequestId, string UserName, string SwineName, int SwineWeight, string Tag, long Id, string CallerUserName, string CallerSwineName, int CallerSwineWeight, int WinChance, int DeclinePenalty) : ViewModel;

public record DuelRequestSentViewModel(string UserName, string SwineName) : ViewModel;

public record TooSoonToDuelViewModel(string SwineName, string LastOpponentName, int HoursLeft) : ViewModel;

public record DuelCancelViewModel(bool HadActiveDuel, string OpponentName) : ViewModel;
