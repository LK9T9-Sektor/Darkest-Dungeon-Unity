using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// Replays a recorded estate run: resources, roster, buildings, quests, town events and the
    /// action timeline. The state is projected from <see cref="EstateSnapshot"/> events, never
    /// simulated.
    /// </summary>
    public partial class EstateReplayViewModel : ObservableObject
    {
        private const string EmptyValue = "—";
        private static readonly TimeSpan EventPacing = TimeSpan.FromMilliseconds(100);

        private readonly INavigationService navigation;
        private readonly GameEventLogReader reader;
        private readonly ILogger logger;
        private readonly IReplayPacer pacer;
        private readonly string captureDirectory;
        private readonly Dictionary<Type, Action<GameEvent>> stateHandlers;
        private readonly Dictionary<Type, Func<GameEvent, string>> descriptions;

        private GameEventCapture? capture;
        private IReadOnlyList<GameEvent> events = Array.Empty<GameEvent>();
        private int currentIndex;

        /// <summary>Gets or sets the current campaign week.</summary>
        [ObservableProperty]
        private int _week;

        /// <summary>Gets or sets the active town event line.</summary>
        [ObservableProperty]
        private string _townEventText = EmptyValue;

        /// <summary>Gets or sets the replay status line.</summary>
        [ObservableProperty]
        private string _statusText = string.Empty;

        /// <summary>Gets or sets the reader error shown when the capture cannot be loaded.</summary>
        [ObservableProperty]
        private string _errorText = string.Empty;

        /// <summary>Gets or sets a value indicating whether the replay is running.</summary>
        [ObservableProperty]
        private bool _isPlaying;

        /// <summary>Gets or sets a value indicating whether the replay reached the last event.</summary>
        [ObservableProperty]
        private bool _isFinished;

        /// <summary>Gets or sets the 1-based number of the event being applied.</summary>
        [ObservableProperty]
        private int _currentEventNumber;

        /// <summary>Gets or sets the timeline line of the event being applied.</summary>
        [ObservableProperty]
        private CaptureTimelineEntryViewModel? _currentEntry;

        /// <summary>Gets the gold and heirloom resources.</summary>
        public ObservableCollection<EstateResourceViewModel> Resources { get; } = new ObservableCollection<EstateResourceViewModel>();

        /// <summary>Gets the estate buildings.</summary>
        public ObservableCollection<EstateBuildingViewModel> Buildings { get; } = new ObservableCollection<EstateBuildingViewModel>();

        /// <summary>Gets the estate roster.</summary>
        public ObservableCollection<EstateHeroViewModel> Roster { get; } = new ObservableCollection<EstateHeroViewModel>();

        /// <summary>Gets the available quests.</summary>
        public ObservableCollection<EstateQuestViewModel> Quests { get; } = new ObservableCollection<EstateQuestViewModel>();

        /// <summary>Gets the full replay timeline.</summary>
        public ObservableCollection<CaptureTimelineEntryViewModel> Timeline { get; } = new ObservableCollection<CaptureTimelineEntryViewModel>();

        /// <summary>Gets the audio events reached by the replay.</summary>
        public ObservableCollection<CaptureTimelineEntryViewModel> AudioLog { get; } = new ObservableCollection<CaptureTimelineEntryViewModel>();

        /// <summary>Gets the screen title.</summary>
        public string ScreenTitle => "ИМЕНИЕ: ЗАХВАТ";

        /// <summary>Gets the total number of events in the capture.</summary>
        public int TotalEvents => events.Count;

        /// <summary>Gets the capture subtitle (notes and event count).</summary>
        public string CaptureSubtitle
        {
            get
            {
                string notes = capture?.Manifest.Notes ?? "estate capture";
                return $"{notes} · {TotalEvents} событий";
            }
        }

        /// <summary>Gets the command restarting the replay from the first event.</summary>
        public IRelayCommand ReplayCommand { get; }

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="EstateReplayViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="reader">The JSONL capture reader.</param>
        /// <param name="logger">The structural logger.</param>
        /// <param name="captureDirectory">The directory with <c>manifest.json</c> and the JSONL files.</param>
        /// <param name="pacer">The replay pacing scheduler.</param>
        public EstateReplayViewModel(
            INavigationService navigation,
            GameEventLogReader reader,
            ILogger logger,
            string captureDirectory,
            IReplayPacer pacer)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.captureDirectory = captureDirectory ?? throw new ArgumentNullException(nameof(captureDirectory));
            this.pacer = pacer ?? throw new ArgumentNullException(nameof(pacer));

            stateHandlers = new Dictionary<Type, Action<GameEvent>>
            {
                [typeof(EstateSnapshot)] = gameEvent => ApplySnapshot((EstateSnapshot)gameEvent),
                [typeof(AudioPlayed)] = gameEvent => ApplyAudio((AudioPlayed)gameEvent),
            };

            descriptions = new Dictionary<Type, Func<GameEvent, string>>
            {
                [typeof(EstateSnapshot)] = gameEvent => DescribeSnapshot((EstateSnapshot)gameEvent),
                [typeof(EstateHeroRecruited)] = gameEvent => DescribeHeroRecruited((EstateHeroRecruited)gameEvent),
                [typeof(EstateHeroDismissed)] = gameEvent => DescribeHeroDismissed((EstateHeroDismissed)gameEvent),
                [typeof(EstateHeroResurrected)] = gameEvent => DescribeHeroResurrected((EstateHeroResurrected)gameEvent),
                [typeof(EstateBuildingUpgraded)] = gameEvent => DescribeBuildingUpgraded((EstateBuildingUpgraded)gameEvent),
                [typeof(EstateQuestSelected)] = gameEvent => DescribeQuestSelected((EstateQuestSelected)gameEvent),
                [typeof(EstateProvisionsPurchased)] = gameEvent => DescribeProvisionsPurchased((EstateProvisionsPurchased)gameEvent),
                [typeof(EstateHeirloomExchanged)] = gameEvent => DescribeHeirloomExchanged((EstateHeirloomExchanged)gameEvent),
                [typeof(EstateTownEventTriggered)] = gameEvent => DescribeTownEvent((EstateTownEventTriggered)gameEvent),
                [typeof(AudioPlayed)] = gameEvent => DescribeAudio((AudioPlayed)gameEvent),
            };

            ReplayCommand = new RelayCommand(StartReplay, () => capture != null && !IsPlaying);
            BackCommand = new RelayCommand(GoBack);

            LoadCapture();
        }

        private void LoadCapture()
        {
            Result<GameEventCapture> result = reader.Read(captureDirectory);
            if (!result.IsSuccess)
            {
                ErrorText = result.ErrorMessage;
                StatusText = "Захват имения недоступен";
                logger.Warn($"estate replay: cannot read '{captureDirectory}': {result.ErrorMessage}");
                return;
            }

            capture = result.Value;
            events = capture.Events;
            BuildTimeline();
            OnPropertyChanged(nameof(TotalEvents));
            OnPropertyChanged(nameof(CaptureSubtitle));
            logger.Log($"estate replay: loaded '{captureDirectory}' events={events.Count}");
            StartReplay();
        }

        private void BuildTimeline()
        {
            Timeline.Clear();
            foreach (GameEvent gameEvent in events)
            {
                string description = descriptions.TryGetValue(gameEvent.GetType(), out Func<GameEvent, string>? describe)
                    ? describe(gameEvent)
                    : gameEvent.GetType().Name;
                Timeline.Add(new CaptureTimelineEntryViewModel(gameEvent.Sequence, description));
            }
        }

        private void StartReplay()
        {
            if (capture == null)
                return;

            pacer.Cancel();
            ResetState();

            currentIndex = 0;
            IsFinished = false;
            IsPlaying = true;
            ReplayCommand.NotifyCanExecuteChanged();
            ErrorText = string.Empty;
            StatusText = $"Событие 1 из {TotalEvents}";
            logger.Log($"estate replay: started events={TotalEvents}");
            Advance();
        }

        private void ResetState()
        {
            Resources.Clear();
            Buildings.Clear();
            Roster.Clear();
            Quests.Clear();
            AudioLog.Clear();
            Week = 0;
            TownEventText = EmptyValue;
            CurrentEventNumber = 0;
            CurrentEntry = null;
            foreach (CaptureTimelineEntryViewModel entry in Timeline)
            {
                entry.IsCurrent = false;
                entry.IsPast = false;
            }
        }

        private void Advance()
        {
            while (currentIndex < events.Count)
            {
                GameEvent gameEvent = events[currentIndex];
                currentIndex++;
                MarkCurrent(currentIndex - 1);
                ApplyEvent(gameEvent);
                logger.Log($"estate replay: event seq={gameEvent.Sequence} type={gameEvent.GetType().Name}");

                if (currentIndex < events.Count)
                {
                    pacer.Schedule(EventPacing, Advance);
                    return;
                }
            }

            Finish();
        }

        private void ApplyEvent(GameEvent gameEvent)
        {
            if (stateHandlers.TryGetValue(gameEvent.GetType(), out Action<GameEvent>? handler))
                handler(gameEvent);
        }

        private void MarkCurrent(int index)
        {
            for (int i = 0; i < Timeline.Count; i++)
            {
                Timeline[i].IsCurrent = i == index;
                Timeline[i].IsPast = i < index;
            }

            CurrentEventNumber = index + 1;
            CurrentEntry = Timeline[index];
            StatusText = $"Событие {CurrentEventNumber} из {TotalEvents}";
        }

        private void Finish()
        {
            IsPlaying = false;
            IsFinished = true;
            StatusText = $"Готово: {TotalEvents} событий";
            ReplayCommand.NotifyCanExecuteChanged();
            logger.Log("estate replay: finished");
        }

        private void ApplySnapshot(EstateSnapshot snapshot)
        {
            Week = snapshot.Week;
            Resources.Clear();
            if (snapshot.Resources != null)
            {
                foreach (EstateResourceAmount resource in snapshot.Resources)
                    Resources.Add(new EstateResourceViewModel(resource));
            }

            Buildings.Clear();
            if (snapshot.Buildings != null)
            {
                foreach (EstateBuildingState building in snapshot.Buildings)
                    Buildings.Add(new EstateBuildingViewModel(building));
            }

            Roster.Clear();
            if (snapshot.Roster != null)
            {
                foreach (EstateHeroState hero in snapshot.Roster)
                    Roster.Add(new EstateHeroViewModel(hero));
            }

            Quests.Clear();
            if (snapshot.Quests != null)
            {
                foreach (EstateQuestState quest in snapshot.Quests)
                    Quests.Add(new EstateQuestViewModel(quest));
            }

            TownEventText = string.IsNullOrEmpty(snapshot.TownEventId)
                ? EmptyValue
                : $"{snapshot.TownEventId} ({snapshot.TownEventTone})";
        }

        private void ApplyAudio(AudioPlayed audio)
        {
            AudioLog.Add(new CaptureTimelineEntryViewModel(
                audio.Sequence,
                $"{audio.EventPath} · {audio.Kind} · {audio.Action}"));
        }

        private static string DescribeSnapshot(EstateSnapshot snapshot)
        {
            string gold = "—";
            if (snapshot.Resources != null)
            {
                EstateResourceAmount? resource = snapshot.Resources.FirstOrDefault(
                    item => item.Id == EstateResourceIds.Gold);
                if (resource != null)
                    gold = resource.Amount.ToString();
            }

            int heroes = snapshot.Roster != null ? snapshot.Roster.Count : 0;
            return $"Имение · неделя {snapshot.Week} · золото {gold} · героев {heroes}";
        }

        private static string DescribeHeroRecruited(EstateHeroRecruited hero)
        {
            return $"Принят {hero.Name} ({hero.Class})";
        }

        private static string DescribeHeroDismissed(EstateHeroDismissed hero)
        {
            return $"Уволен {hero.Name} ({hero.Class})";
        }

        private static string DescribeHeroResurrected(EstateHeroResurrected hero)
        {
            return $"Воскрешён {hero.Name} ({hero.Class})";
        }

        private static string DescribeBuildingUpgraded(EstateBuildingUpgraded upgrade)
        {
            return $"Апгрейд {upgrade.BuildingId}: {upgrade.TreeId}/{upgrade.UpgradeId}";
        }

        private static string DescribeQuestSelected(EstateQuestSelected quest)
        {
            return $"Выбран квест {quest.Dungeon}/{quest.Type}";
        }

        private static string DescribeProvisionsPurchased(EstateProvisionsPurchased purchase)
        {
            string action = purchase.IsSellback ? "продано" : "куплено";
            return $"{action} {purchase.ItemId} x{purchase.Amount}";
        }

        private static string DescribeHeirloomExchanged(EstateHeirloomExchanged exchange)
        {
            return $"Обмен {exchange.FromId} x{exchange.FromAmount} → {exchange.ToId} x{exchange.ToAmount}";
        }

        private static string DescribeTownEvent(EstateTownEventTriggered townEvent)
        {
            return $"Городское событие: {townEvent.TownEventId} ({townEvent.Tone})";
        }

        private static string DescribeAudio(AudioPlayed audio)
        {
            return $"{audio.EventPath} · {audio.Kind} · {audio.Action}";
        }

        private void GoBack()
        {
            pacer.Cancel();
            navigation.GoHome();
        }
    }
}
