using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// Replays a recorded Unity run as a visual demonstration: restores the party/room/torch/HP from
    /// the snapshots and plays the captured <c>attackExecuted</c> events one by one. Presentation
    /// only - the combat state is projected, never simulated.
    /// </summary>
    public partial class CaptureReplayViewModel : ObservableObject
    {
        private const string EmptyValue = "—";
        private const string HpStat = "hp";
        private const string StressStat = "stress";
        private static readonly TimeSpan EventPacing = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan AttackFallbackDelay = TimeSpan.FromMilliseconds(900);
        private static readonly TimeSpan MinimumAttackDuration = TimeSpan.FromMilliseconds(1200);

        private readonly INavigationService navigation;
        private readonly ICaptureAnimationCatalog catalog;
        private readonly ILogger logger;
        private readonly GameEventLogReader reader;
        private readonly IReplayPacer pacer;
        private readonly string captureDirectory;
        private readonly Dictionary<string, CaptureUnitViewModel> units =
            new Dictionary<string, CaptureUnitViewModel>(StringComparer.Ordinal);
        private readonly Dictionary<Type, Action<GameEvent>> stateHandlers;
        private readonly Dictionary<Type, Func<GameEvent, string>> descriptions;
        private readonly Stopwatch replayClock = new Stopwatch();
        private readonly Stopwatch attackClock = new Stopwatch();

        private GameEventCapture? capture;
        private IReadOnlyList<GameEvent> events = Array.Empty<GameEvent>();
        private int currentIndex;
        private long currentAttackSequence;
        private bool _pendingFinishAttack;

        /// <summary>Gets or sets the scene name of the capture.</summary>
        [ObservableProperty]
        private string _sceneName = EmptyValue;

        /// <summary>Gets or sets the current room id.</summary>
        [ObservableProperty]
        private string _roomName = EmptyValue;

        /// <summary>Gets or sets the current room type.</summary>
        [ObservableProperty]
        private string _roomType = string.Empty;

        /// <summary>Gets or sets the current torch value.</summary>
        [ObservableProperty]
        private int _torch;

        /// <summary>Gets or sets the current gold amount.</summary>
        [ObservableProperty]
        private int _gold;

        /// <summary>Gets or sets the current combat round.</summary>
        [ObservableProperty]
        private int _round;

        /// <summary>Gets or sets the current combat turn kind.</summary>
        [ObservableProperty]
        private string _turn = EmptyValue;

        /// <summary>Gets or sets the final battle result line.</summary>
        [ObservableProperty]
        private string _battleWinner = EmptyValue;

        /// <summary>Gets or sets the replay status line.</summary>
        [ObservableProperty]
        private string _statusText = string.Empty;

        /// <summary>Gets or sets the reader error shown when the capture cannot be loaded.</summary>
        [ObservableProperty]
        private string _errorText = string.Empty;

        /// <summary>Gets or sets a value indicating whether the replay is running.</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReplayCommand))]
        [NotifyCanExecuteChangedFor(nameof(TogglePauseCommand))]
        private bool _isPlaying;

        /// <summary>Gets or sets a value indicating whether the running replay is paused.</summary>
        [ObservableProperty]
        private bool _isPaused;

        /// <summary>Gets or sets a value indicating whether the replay reached the last event.</summary>
        [ObservableProperty]
        private bool _isFinished;

        /// <summary>Gets or sets a value indicating whether the replay is waiting for an attack animation.</summary>
        [ObservableProperty]
        private bool _isAwaitingAnimation;

        /// <summary>Gets or sets the 1-based number of the event being applied.</summary>
        [ObservableProperty]
        private int _currentEventNumber;

        /// <summary>Gets or sets the timeline line of the event being applied.</summary>
        [ObservableProperty]
        private CaptureTimelineEntryViewModel? _currentEntry;

        /// <summary>Gets the party members restored from the nearest snapshot.</summary>
        public ObservableCollection<CaptureUnitViewModel> Party { get; } = new ObservableCollection<CaptureUnitViewModel>();

        /// <summary>Gets the party in display order (rank 1 nearest the enemies).</summary>
        public ObservableCollection<CaptureUnitViewModel> PartyFormation { get; } = new ObservableCollection<CaptureUnitViewModel>();

        /// <summary>Gets the monsters restored from the nearest snapshot.</summary>
        public ObservableCollection<CaptureUnitViewModel> Enemies { get; } = new ObservableCollection<CaptureUnitViewModel>();

        /// <summary>Gets the full replay timeline.</summary>
        public ObservableCollection<CaptureTimelineEntryViewModel> Timeline { get; } = new ObservableCollection<CaptureTimelineEntryViewModel>();

        /// <summary>Gets the audio events reached by the replay.</summary>
        public ObservableCollection<CaptureTimelineEntryViewModel> AudioLog { get; } = new ObservableCollection<CaptureTimelineEntryViewModel>();

        /// <summary>Gets the screen title.</summary>
        public string ScreenTitle => "ВОСПРОИЗВЕДЕНИЕ ЗАХВАТА";

        /// <summary>Gets the total number of events in the capture.</summary>
        public int TotalEvents => events.Count;

        /// <summary>Gets the capture subtitle (notes and event count).</summary>
        public string CaptureSubtitle
        {
            get
            {
                string notes = capture?.Manifest.Notes ?? "reference capture";
                return $"{notes} · {TotalEvents} событий";
            }
        }

        /// <summary>Gets the command restarting the replay from the first event.</summary>
        public IRelayCommand ReplayCommand { get; }

        /// <summary>Gets the command pausing or resuming the running replay.</summary>
        public IRelayCommand TogglePauseCommand { get; }

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>
        /// Gets the command executed by the attacker control when its non-looping animation
        /// finished; the parameter is the completed animation name.
        /// </summary>
        public IRelayCommand<string?> AnimationCompletedCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="CaptureReplayViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="reader">The JSONL capture reader.</param>
        /// <param name="catalog">The catalog resolving spine animation content.</param>
        /// <param name="logger">The structural logger.</param>
        /// <param name="captureDirectory">The directory with <c>manifest.json</c> and the JSONL files.</param>
        /// <param name="pacer">The replay pacing scheduler.</param>
        public CaptureReplayViewModel(
            INavigationService navigation,
            GameEventLogReader reader,
            ICaptureAnimationCatalog catalog,
            ILogger logger,
            string captureDirectory,
            IReplayPacer pacer)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.captureDirectory = captureDirectory ?? throw new ArgumentNullException(nameof(captureDirectory));
            this.pacer = pacer ?? throw new ArgumentNullException(nameof(pacer));

            stateHandlers = new Dictionary<Type, Action<GameEvent>>
            {
                [typeof(SceneSnapshot)] = gameEvent => ApplySceneSnapshot((SceneSnapshot)gameEvent),
                [typeof(StateSnapshot)] = gameEvent => ApplyStateSnapshot((StateSnapshot)gameEvent),
                [typeof(PartyMoved)] = gameEvent => ApplyPartyMoved((PartyMoved)gameEvent),
                [typeof(RoomDiscovered)] = gameEvent => ApplyRoomDiscovered((RoomDiscovered)gameEvent),
                [typeof(TorchChanged)] = gameEvent => ApplyTorchChanged((TorchChanged)gameEvent),
                [typeof(HeroStatChanged)] = gameEvent => ApplyHeroStatChanged((HeroStatChanged)gameEvent),
                [typeof(SkillResolved)] = gameEvent => ApplySkillResolved((SkillResolved)gameEvent),
                [typeof(UnitMoved)] = gameEvent => ApplyUnitMoved((UnitMoved)gameEvent),
                [typeof(TurnStarted)] = gameEvent => ApplyTurnStarted((TurnStarted)gameEvent),
                [typeof(RoundStarted)] = gameEvent => ApplyRoundStarted((RoundStarted)gameEvent),
                [typeof(BattleEnded)] = gameEvent => ApplyBattleEnded((BattleEnded)gameEvent),
                [typeof(AudioPlayed)] = gameEvent => ApplyAudioPlayed((AudioPlayed)gameEvent),
            };

            descriptions = new Dictionary<Type, Func<GameEvent, string>>
            {
                [typeof(SceneSnapshot)] = gameEvent => DescribeSceneSnapshot((SceneSnapshot)gameEvent),
                [typeof(StateSnapshot)] = gameEvent => DescribeStateSnapshot((StateSnapshot)gameEvent),
                [typeof(PartyMoved)] = gameEvent => DescribePartyMoved((PartyMoved)gameEvent),
                [typeof(RoomDiscovered)] = gameEvent => DescribeRoomDiscovered((RoomDiscovered)gameEvent),
                [typeof(TorchChanged)] = gameEvent => DescribeTorchChanged((TorchChanged)gameEvent),
                [typeof(HeroStatChanged)] = gameEvent => DescribeHeroStatChanged((HeroStatChanged)gameEvent),
                [typeof(AttackExecuted)] = gameEvent => DescribeAttackExecuted((AttackExecuted)gameEvent),
                [typeof(SkillUnavailable)] = gameEvent => DescribeSkillUnavailable((SkillUnavailable)gameEvent),
                [typeof(ActionRejected)] = gameEvent => DescribeActionRejected((ActionRejected)gameEvent),
                [typeof(SkillResolved)] = gameEvent => DescribeSkillResolved((SkillResolved)gameEvent),
                [typeof(UnitMoved)] = gameEvent => DescribeUnitMoved((UnitMoved)gameEvent),
                [typeof(TurnStarted)] = gameEvent => DescribeTurnStarted((TurnStarted)gameEvent),
                [typeof(TurnEnded)] = gameEvent => DescribeTurnEnded((TurnEnded)gameEvent),
                [typeof(RoundStarted)] = gameEvent => DescribeRoundStarted((RoundStarted)gameEvent),
                [typeof(RoundEnded)] = gameEvent => DescribeRoundEnded((RoundEnded)gameEvent),
                [typeof(BattleEnded)] = gameEvent => DescribeBattleEnded((BattleEnded)gameEvent),
                [typeof(AudioPlayed)] = gameEvent => DescribeAudioPlayed((AudioPlayed)gameEvent),
            };

            ReplayCommand = new RelayCommand(StartReplay, () => capture != null && !IsPlaying);
            TogglePauseCommand = new RelayCommand(TogglePause, () => IsPlaying);
            BackCommand = new RelayCommand(GoBack);
            AnimationCompletedCommand = new RelayCommand<string?>(OnAnimationCompleted);

            LoadCapture();
        }

        private void LoadCapture()
        {
            Result<GameEventCapture> result = reader.Read(captureDirectory);
            if (!result.IsSuccess)
            {
                ErrorText = result.ErrorMessage;
                StatusText = "Захват недоступен";
                logger.Warn($"capture replay: cannot read '{captureDirectory}': {result.ErrorMessage}");
                return;
            }

            capture = result.Value;
            events = capture.Events;
            BuildTimeline();
            OnPropertyChanged(nameof(TotalEvents));
            OnPropertyChanged(nameof(CaptureSubtitle));
            ReplayCommand.NotifyCanExecuteChanged();
            logger.Log($"capture replay: loaded '{captureDirectory}' events={events.Count}");
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
            attackClock.Reset();
            IsAwaitingAnimation = false;
            IsPaused = false;
            _pendingFinishAttack = false;
            ResetState();

            currentIndex = 0;
            IsFinished = false;
            IsPlaying = true;
            ErrorText = string.Empty;
            StatusText = $"Событие 1 из {TotalEvents}";
            replayClock.Restart();
            logger.Log($"capture replay: started events={TotalEvents}");
            Advance();
        }

        private void TogglePause()
        {
            if (!IsPlaying)
                return;

            if (IsPaused)
                ResumeReplay();
            else
                PauseReplay();
        }

        private void PauseReplay()
        {
            IsPaused = true;

            // Keep the fallback timer alive while an attack has no linked spine content;
            // otherwise resuming would wait for a completion callback that no longer exists.
            if (!IsAwaitingAnimation)
                pacer.Cancel();

            StatusText = $"Пауза — событие {CurrentEventNumber} из {TotalEvents}";
            logger.Log("capture replay: paused");
        }

        private void ResumeReplay()
        {
            IsPaused = false;
            StatusText = $"Событие {CurrentEventNumber} из {TotalEvents}";
            logger.Log("capture replay: resumed");

            if (_pendingFinishAttack)
            {
                FinishAttack();
                return;
            }

            if (IsAwaitingAnimation)
                return;

            if (currentIndex < events.Count)
            {
                pacer.Schedule(EventPacing, Advance);
                return;
            }

            Finish();
        }

        private void ResetState()
        {
            units.Clear();
            Party.Clear();
            PartyFormation.Clear();
            Enemies.Clear();
            AudioLog.Clear();
            SceneName = EmptyValue;
            RoomName = EmptyValue;
            RoomType = string.Empty;
            Torch = 0;
            Gold = 0;
            Round = 0;
            Turn = EmptyValue;
            BattleWinner = EmptyValue;
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
                logger.Log($"capture replay: event seq={gameEvent.Sequence} type={gameEvent.GetType().Name}");

                if (gameEvent is AttackExecuted attack)
                {
                    BeginAttack(attack);
                    return;
                }

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
            pacer.Cancel();
            IsPlaying = false;
            IsPaused = false;
            IsFinished = true;
            IsAwaitingAnimation = false;
            _pendingFinishAttack = false;
            StatusText = $"Готово: {TotalEvents} событий";
            logger.Log($"capture replay: finished events={TotalEvents} elapsedMs={replayClock.Elapsed.TotalMilliseconds:0}");
        }

        private void ApplySceneSnapshot(SceneSnapshot snapshot)
        {
            SceneName = snapshot.SceneName;
            ApplyGlobals(snapshot.Globals);
            SetCombatants(snapshot.Heroes, snapshot.Party);
        }

        private void ApplyStateSnapshot(StateSnapshot snapshot)
        {
            ApplyGlobals(snapshot.Globals);
            SetCombatants(snapshot.Combatants, null);
            ApplyBattleState(snapshot.Battle);
        }

        private void ApplyBattleState(BattleState? battle)
        {
            if (battle == null)
                return;

            Round = battle.Round;
            Turn = string.IsNullOrEmpty(battle.Turn) ? EmptyValue : battle.Turn!;
        }

        private void ApplyGlobals(SceneGlobals globals)
        {
            Torch = globals.Torch;
            Gold = globals.Gold;
        }

        private void SetCombatants(IReadOnlyList<CombatantState> states, IReadOnlyList<string>? partyOrder)
        {
            List<string> previousHeroOrder = Party.Select(unit => unit.Id).ToList();
            units.Clear();
            Party.Clear();
            PartyFormation.Clear();
            Enemies.Clear();

            for (int index = 0; index < states.Count; index++)
            {
                CombatantState state = states[index];
                var unit = new CaptureUnitViewModel(state, index + 1);
                unit.SetIdleContent(catalog.GetIdleDirectory(state.Skeleton));
                units[state.Id] = unit;
            }

            var added = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in partyOrder ?? previousHeroOrder)
            {
                if (units.TryGetValue(id, out CaptureUnitViewModel? unit) && !unit.IsEnemy && added.Add(id))
                    Party.Add(unit);
            }

            foreach (CombatantState state in states)
            {
                CaptureUnitViewModel unit = units[state.Id];
                if (unit.IsEnemy)
                {
                    Enemies.Add(unit);
                }
                else if (added.Add(unit.Id))
                {
                    Party.Add(unit);
                }
            }

            SortFormation();
        }

        private void SortFormation()
        {
            List<CaptureUnitViewModel> orderedParty = Party.OrderBy(unit => unit.Rank).ToList();
            Party.Clear();
            foreach (CaptureUnitViewModel unit in orderedParty)
                Party.Add(unit);

            List<CaptureUnitViewModel> orderedEnemies = Enemies.OrderBy(unit => unit.Rank).ToList();
            Enemies.Clear();
            foreach (CaptureUnitViewModel unit in orderedEnemies)
                Enemies.Add(unit);

            PartyFormation.Clear();
            foreach (CaptureUnitViewModel unit in Party.Reverse())
                PartyFormation.Add(unit);
        }

        private void ApplyPartyMoved(PartyMoved moved)
        {
            RoomName = moved.ToId;
        }

        private void ApplyRoomDiscovered(RoomDiscovered discovered)
        {
            RoomName = discovered.RoomId;
            RoomType = discovered.RoomType;
        }

        private void ApplyTorchChanged(TorchChanged torch)
        {
            Torch = torch.NewValue;
        }

        private void ApplyHeroStatChanged(HeroStatChanged stat)
        {
            if (!units.TryGetValue(stat.HeroId, out CaptureUnitViewModel? unit))
                return;

            if (string.Equals(stat.Stat, HpStat, StringComparison.Ordinal))
            {
                unit.Hp = stat.NewValue;
            }
            else if (string.Equals(stat.Stat, StressStat, StringComparison.Ordinal))
            {
                unit.Stress = stat.NewValue;
            }
        }

        private void ApplySkillResolved(SkillResolved skill)
        {
            bool ranksChanged = false;
            foreach (SkillTargetResult target in skill.Targets)
            {
                if (!units.TryGetValue(target.TargetId, out CaptureUnitViewModel? unit))
                    continue;

                unit.Hp = target.HpAfter;
                unit.Stress = target.StressAfter;
                if (target.RankAfter.HasValue && unit.Rank != target.RankAfter.Value)
                {
                    unit.Rank = target.RankAfter.Value;
                    ranksChanged = true;
                }
            }

            if (ranksChanged)
                SortFormation();
        }

        private void ApplyUnitMoved(UnitMoved moved)
        {
            if (!units.TryGetValue(moved.UnitId, out CaptureUnitViewModel? unit))
                return;

            unit.Rank = moved.ToRank;
            SortFormation();
        }

        private void ApplyTurnStarted(TurnStarted turn)
        {
            Round = turn.Round;
            Turn = string.IsNullOrEmpty(turn.Turn) ? EmptyValue : turn.Turn!;
        }

        private void ApplyRoundStarted(RoundStarted round)
        {
            Round = round.Round;
        }

        private void ApplyBattleEnded(BattleEnded ended)
        {
            if (ended.Retreat)
            {
                BattleWinner = "ОТСТУПЛЕНИЕ";
                return;
            }

            BattleWinner = ended.Winner switch
            {
                BattleWinners.Heroes => "ПОБЕДА ГЕРОЕВ",
                BattleWinners.Monsters => "ПОРАЖЕНИЕ",
                _ => "ИТОГ НЕИЗВЕСТЕН",
            };
        }

        private void ApplyAudioPlayed(AudioPlayed audio)
        {
            AudioLog.Add(new CaptureTimelineEntryViewModel(audio.Sequence, DescribeAudioPlayed(audio)));
        }

        private void BeginAttack(AttackExecuted attack)
        {
            units.TryGetValue(attack.AttackerId, out CaptureUnitViewModel? attacker);
            units.TryGetValue(attack.TargetId, out CaptureUnitViewModel? target);

            ClearHighlights();
            if (attacker != null)
                attacker.IsAttacker = true;
            if (target != null)
                target.IsTarget = true;

            string? attackerContent = attacker == null
                ? null
                : catalog.GetAnimationDirectory(attack.AttackerAnimation) ?? catalog.GetIdleDirectory(attacker.Skeleton);
            attacker?.BeginAttack(attackerContent);
            target?.EndAttack();

            currentAttackSequence = attack.Sequence;
            attackClock.Restart();
            _pendingFinishAttack = false;
            IsAwaitingAnimation = true;
            logger.Log(
                $"capture replay: attack seq={attack.Sequence} attacker={attack.AttackerId} target={attack.TargetId} " +
                $"skill={attack.SkillName} content={attackerContent ?? "none"}");

            if (string.IsNullOrEmpty(attackerContent))
                pacer.Schedule(AttackFallbackDelay, () => OnAnimationCompleted(null));
        }

        private void OnAnimationCompleted(string? animationName)
        {
            if (!IsAwaitingAnimation)
                return;

            IsAwaitingAnimation = false;
            pacer.Cancel();
            TimeSpan elapsed = attackClock.Elapsed;
            TimeSpan remaining = MinimumAttackDuration - elapsed;
            logger.Log($"capture replay: animation completed seq={currentAttackSequence} elapsedMs={elapsed.TotalMilliseconds:0}");

            logger.Log(
                $"capture replay: attack hold seq={currentAttackSequence} waitMs={Math.Max(0, remaining.TotalMilliseconds):0}");
            QueueFinishAttack(remaining);
        }

        private void QueueFinishAttack(TimeSpan delay)
        {
            _pendingFinishAttack = true;
            if (IsPaused)
                return;

            if (delay > TimeSpan.Zero)
            {
                pacer.Schedule(delay, FinishAttack);
                return;
            }

            FinishAttack();
        }

        private void FinishAttack()
        {
            _pendingFinishAttack = false;
            ClearHighlights();
            foreach (CaptureUnitViewModel unit in units.Values)
                unit.EndAttack();

            Advance();
        }

        private void ClearHighlights()
        {
            foreach (CaptureUnitViewModel unit in units.Values)
            {
                unit.IsAttacker = false;
                unit.IsTarget = false;
            }
        }

        private void GoBack()
        {
            pacer.Cancel();
            IsPaused = false;
            IsAwaitingAnimation = false;
            _pendingFinishAttack = false;
            navigation.GoHome();
        }

        private static string DescribeSceneSnapshot(SceneSnapshot snapshot)
        {
            return $"Сцена {snapshot.SceneName} · факел {snapshot.Globals.Torch} · золото {snapshot.Globals.Gold}";
        }

        private static string DescribeStateSnapshot(StateSnapshot snapshot)
        {
            string battle = snapshot.Battle == null ? string.Empty : $" · раунд {snapshot.Battle.Round}";
            return $"Снапшот {snapshot.Context} · факел {snapshot.Globals.Torch} · участников {snapshot.Combatants.Count}{battle}";
        }

        private static string DescribePartyMoved(PartyMoved moved)
        {
            return $"Переход {moved.FromId} → {moved.ToId}";
        }

        private static string DescribeRoomDiscovered(RoomDiscovered discovered)
        {
            return $"Комната {discovered.RoomId} · {discovered.RoomType}";
        }

        private static string DescribeTorchChanged(TorchChanged torch)
        {
            return $"Факел {torch.OldValue} → {torch.NewValue} ({torch.Reason})";
        }

        private static string DescribeHeroStatChanged(HeroStatChanged stat)
        {
            return $"{stat.HeroId} · {stat.Stat} {stat.OldValue} → {stat.NewValue}";
        }

        private static string DescribeAttackExecuted(AttackExecuted attack)
        {
            return $"{attack.AttackerId} → {attack.TargetId} · {attack.SkillName}";
        }

        private static string DescribeSkillUnavailable(SkillUnavailable skill)
        {
            return $"Скилл {skill.Skill} недоступен · {skill.Reason}";
        }

        private static string DescribeActionRejected(ActionRejected action)
        {
            return $"Действие отклонено · {action.Reason}";
        }

        private static string DescribeSkillResolved(SkillResolved skill)
        {
            string targets = skill.Targets.Count == 0
                ? "нет целей"
                : string.Join(", ", skill.Targets.Select(target => $"{target.TargetId}:{target.ResultType} {target.Amount}"));
            return $"{skill.AttackerId} · {skill.SkillName} → {targets}";
        }

        private static string DescribeUnitMoved(UnitMoved moved)
        {
            return $"{moved.UnitId}: ранг {moved.FromRank} → {moved.ToRank} ({moved.Cause})";
        }

        private static string DescribeTurnStarted(TurnStarted turn)
        {
            return $"Ход {turn.ActorId} · раунд {turn.Round}";
        }

        private static string DescribeTurnEnded(TurnEnded turn)
        {
            return $"Конец хода {turn.ActorId} · раунд {turn.Round}";
        }

        private static string DescribeRoundStarted(RoundStarted round)
        {
            return $"Раунд {round.Round} · порядок {round.OrderedUnitIds?.Count ?? 0}";
        }

        private static string DescribeRoundEnded(RoundEnded round)
        {
            return $"Конец раунда {round.Round}";
        }

        private static string DescribeBattleEnded(BattleEnded ended)
        {
            return $"Бой окончен · {ended.Winner} · раундов {ended.Rounds}";
        }

        private static string DescribeAudioPlayed(AudioPlayed audio)
        {
            return $"{audio.EventPath} · {audio.Kind} · {audio.Action}";
        }
    }
}
