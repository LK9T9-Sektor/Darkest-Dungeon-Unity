using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Live snapshot of a duel: units, skills, status and click inputs wired to the core controller.</summary>
    public partial class DuelBattleViewModel : ObservableObject, IPumpable
    {
        private sealed class PendingPopup
        {
            public int CombatId { get; }
            public string Text { get; }
            public int Priority { get; }

            public PendingPopup(int combatId, string text, int priority = 0)
            {
                CombatId = combatId;
                Text = text;
                Priority = priority;
            }
        }

        private sealed class PendingFlash
        {
            public int CombatId { get; }
            public string Kind { get; }

            public PendingFlash(int combatId, string kind)
            {
                CombatId = combatId;
                Kind = kind;
            }
        }

        private readonly DuelController controller;
        private readonly IDuelRivalLink rivalLink;
        private readonly Action onLeave;
        private readonly DispatcherTimer popupTimer;
        private readonly System.Collections.Generic.List<PendingPopup> pendingPopups =
            new System.Collections.Generic.List<PendingPopup>();
        private readonly System.Collections.Generic.List<PendingFlash> pendingFlashes =
            new System.Collections.Generic.List<PendingFlash>();
        private readonly System.Collections.Generic.Dictionary<int, System.Collections.Generic.Queue<string>> popupQueues =
            new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Queue<string>>();
        private string? selectedSkillId;
        private DuelSkillViewModel? selectedSkill;
        private bool isMoveMode;

        /// <summary>Gets or sets the rival (AI) skill preview shown in the badge during the AI's turn.</summary>
        [ObservableProperty]
        private DuelSkillViewModel? _aiSkillPreview;

        /// <summary>Gets or sets the rival (AI) target preview card highlighted during the AI's turn.</summary>
        [ObservableProperty]
        private DuelUnitViewModel? _aiTargetPreview;

        /// <summary>Gets the currently selected skill (badge source above the acting card), or null.</summary>
        public DuelSkillViewModel? SelectedSkill { get { return selectedSkill; } }
        private readonly FormationDisplayOrder heroOrder = FormationDisplayOrder.HeroSide();
        private readonly FormationDisplayOrder monsterOrder = FormationDisplayOrder.MonsterSide();

        /// <summary>Quest text shown in the top-left panel (no round/actor info there).</summary>
        private const string QuestText = "Defeat the rival party";

        /// <summary>Round number the captured turn order belongs to (-1 until the first snapshot).</summary>
        private int _lastRound = -1;

        /// <summary>Full initiative sequence of the current round, captured when the round changes.
        /// The core pops each unit out of <c>Round.OrderedUnits</c> when its turn is prepped, so the
        /// remaining list alone would drop both the acting unit and everyone who already moved.</summary>
        private List<int> _roundStartOrder = new List<int>();

        /// <summary>Gets the local party unit cards (left ranks).</summary>
        public ObservableCollection<DuelUnitViewModel> Heroes { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the rival party unit cards (right ranks).</summary>
        public ObservableCollection<DuelUnitViewModel> Monsters { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the skill buttons of the acting unit.</summary>
        public ObservableCollection<DuelSkillViewModel> Skills { get; } = new ObservableCollection<DuelSkillViewModel>();

        /// <summary>Gets the current round's turn order strip.</summary>
        public ObservableCollection<DuelTurnEntryViewModel> TurnOrder { get; } = new ObservableCollection<DuelTurnEntryViewModel>();

        /// <summary>Gets the battle log lines.</summary>
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        /// <summary>Gets the events overlay (round number, announcements).</summary>
        public EventsLayerViewModel Events { get; } = new EventsLayerViewModel();

        /// <summary>Gets the quest panel (title, goal).</summary>
        public QuestLogViewModel Quest { get; } = new QuestLogViewModel();

        /// <summary>Gets the bottom raid HUD (actor info, tooltip, log/inventory/map).</summary>
        public RaidHudViewModel RaidHud { get; } = new RaidHudViewModel();

        /// <summary>Gets the top-center torch meter (placeholder for duels).</summary>
        public TorchViewModel Torch { get; } = new TorchViewModel();

        /// <summary>Gets the stat sheet shown when a unit is right-clicked.</summary>
        public HeroStatsViewModel StatsTarget { get; } = new HeroStatsViewModel();

        /// <summary>Gets or sets a value indicating whether the stats sheet overlay is visible.</summary>
        [ObservableProperty]
        private bool _isStatsVisible;

        /// <summary>Gets or sets the unit whose buff/debuff table is displayed.</summary>
        [ObservableProperty]
        private DuelUnitViewModel? _buffTarget;

        /// <summary>Gets or sets a value indicating whether the buff/debuff table overlay is visible.</summary>
        [ObservableProperty]
        private bool _isBuffTableVisible;

        /// <summary>Gets or sets the status line (round / turn / result).</summary>
        [ObservableProperty]
        private string _status = string.Empty;

        /// <summary>Gets the command that selects a skill.</summary>
        public IRelayCommand<DuelSkillViewModel> SelectSkillCommand { get; }

        /// <summary>Gets the command that targets and executes.</summary>
        public IRelayCommand<DuelUnitViewModel> TargetCommand { get; }

        /// <summary>Gets the command that starts a move of the acting unit to an adjacent rank.</summary>
        public IRelayCommand MoveCommand { get; }

        /// <summary>Gets the command that skips the acting unit's turn.</summary>
        public IRelayCommand PassCommand { get; }

        /// <summary>Gets the command that abandons the duel and returns to the main menu.</summary>
        public IRelayCommand LeaveCommand { get; }

        /// <summary>Gets the command that opens the stats sheet for the given unit.</summary>
        public IRelayCommand<DuelUnitViewModel> OpenStatsCommand { get; }

        /// <summary>Gets the command that closes the stats sheet.</summary>
        public IRelayCommand CloseStatsCommand { get; }

        /// <summary>Gets the command that toggles the buff/debuff table for the given unit.</summary>
        public IRelayCommand<DuelUnitViewModel> ToggleBuffTableCommand { get; }

        /// <summary>Gets the command that closes the buff/debuff table.</summary>
        public IRelayCommand CloseBuffTableCommand { get; }

        /// <summary>Gets the team of the unit whose turn is being played.</summary>
        public Team CurrentActorTeam
        {
            get { return controller.CurrentUnit?.Team ?? Team.Heroes; }
        }

        /// <summary>Gets a value indicating whether it is the local player's turn.</summary>
        public bool IsLocalTurn { get { return controller.IsLocalTurn; } }

        /// <summary>Gets a value indicating whether the move mode (adjacent rank swap) is active.</summary>
        public bool IsMoveMode { get { return isMoveMode; } }

        /// <summary>Gets the tone of the selected skill (attack/heal/buff) for the target arrow color.</summary>
        public Ui.SkillTone SelectedSkillTone
        {
            get
            {
                var unit = controller.CurrentUnit;
                var skill = unit?.Character.CurrentCombatSkills?.FirstOrDefault(candidate => candidate.Id == selectedSkillId);
                return Ui.SkillToneClassifier.Classify(skill);
            }
        }

        /// <summary>Gets a value indicating whether the selected skill targets multiple units at once (AOE / party).</summary>
        public bool SelectedSkillIsMultiTarget
        {
            get
            {
                var unit = controller.CurrentUnit;
                var skill = unit?.Character.CurrentCombatSkills?.FirstOrDefault(candidate => candidate.Id == selectedSkillId);
                return skill != null
                    && (skill.TargetRanks != null
                        && (skill.TargetRanks.IsSelfFormation || skill.TargetRanks.Ranks.Count > 1));
            }
        }

        /// <summary>Gets the rank (1-4) of the unit whose turn is being played.</summary>
        public int CurrentActorRank
        {
            get { return controller.CurrentUnit?.Rank ?? 0; }
        }

        /// <summary>Initializes a new instance of the <see cref="DuelBattleViewModel"/> class.</summary>
        /// <param name="controller">The duel controller.</param>
        /// <param name="rivalLink">The rival input channel (network or AI).</param>
        /// <param name="onLeave">Invoked when the player abandons the duel.</param>
        public DuelBattleViewModel(DuelController controller, IDuelRivalLink rivalLink, Action onLeave)
        {
            this.controller = controller;
            this.rivalLink = rivalLink;
            this.onLeave = onLeave;
            SelectSkillCommand = new RelayCommand<DuelSkillViewModel>(SelectSkill);
            TargetCommand = new RelayCommand<DuelUnitViewModel>(SelectTarget);
            MoveCommand = new RelayCommand(SelectMove);
            PassCommand = new RelayCommand(Pass);
            LeaveCommand = new RelayCommand(Leave);
            OpenStatsCommand = new RelayCommand<DuelUnitViewModel>(OpenStats);
            CloseStatsCommand = new RelayCommand(() => IsStatsVisible = false);
            ToggleBuffTableCommand = new RelayCommand<DuelUnitViewModel>(ToggleBuffTable);
            CloseBuffTableCommand = new RelayCommand(() => IsBuffTableVisible = false);
            controller.Events.StateChanged += Refresh;
            controller.Events.PopupShown += OnPopupShown;
            rivalLink.RivalActionReceived += OnRivalActionReceived;
            rivalLink.SkillPreviewed += OnAiSkillPreviewed;
            rivalLink.TargetPreviewed += OnAiTargetPreviewed;
            rivalLink.Attach(controller);
            Quest.Title = "Duel";
            popupTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.4) };
            popupTimer.Tick += (s, e) => AdvancePopups();
            popupTimer.Start();
            Refresh();
        }

        /// <inheritdoc/>
        public void Pump()
        {
            rivalLink.Pump();
        }

        /// <summary>Leaves the duel: detaches the rival link and returns to the main menu.</summary>
        public void Leave()
        {
            controller.Events.StateChanged -= Refresh;
            controller.Events.PopupShown -= OnPopupShown;
            rivalLink.RivalActionReceived -= OnRivalActionReceived;
            rivalLink.SkillPreviewed -= OnAiSkillPreviewed;
            rivalLink.TargetPreviewed -= OnAiTargetPreviewed;
            rivalLink.Dispose();
            onLeave();
        }

        /// <summary>Applies a rival action payload and refreshes the snapshot.</summary>
        /// <param name="payload">The raw action payload ("skillId|targetId").</param>
        public void ApplyRivalInput(string payload)
        {
            string? actor = controller.CurrentUnit?.Character.Name;
            controller.ApplyRemoteSkill(payload);
            LogAction(actor, payload);
            Refresh();
        }

        /// <summary>Rebuilds the snapshot from the core controller.</summary>
        public void Refresh()
        {
            RefreshUnits();
            RefreshSkills();
            RefreshStatus();
            RefreshLog();
            RefreshEvents();
            RefreshActor();
            ApplyPopups();
            AiSkillPreview = null;
            AiTargetPreview = null;
        }

        private void ApplyPopups()
        {
            if (pendingPopups.Count == 0 && pendingFlashes.Count == 0)
                return;

            foreach (var popup in pendingPopups.OrderBy(p => p.Priority))
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == popup.CombatId)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == popup.CombatId);
                if (card == null)
                    continue;

                if (!popupQueues.TryGetValue(popup.CombatId, out var queue))
                {
                    queue = new System.Collections.Generic.Queue<string>();
                    popupQueues[popup.CombatId] = queue;
                }
                queue.Enqueue(popup.Text);

                if (queue.Count == 1)
                {
                    card.DamagePopupText = popup.Text;
                    card.DamagePopupVisible = true;
                }
            }
            pendingPopups.Clear();

            foreach (var flash in pendingFlashes)
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == flash.CombatId)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == flash.CombatId);
                if (card != null)
                    card.CardFlash = flash.Kind;
            }
            pendingFlashes.Clear();
        }

        /// <summary>Advances the per-card popup queues so damage, then bleed/buff/debuff texts play in
        /// sequence instead of being overwritten; hides a card's popup once its queue is drained.</summary>
        private void AdvancePopups()
        {
            foreach (var entry in popupQueues.ToList())
            {
                var queue = entry.Value;
                var card = Heroes.FirstOrDefault(h => h.CombatId == entry.Key)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == entry.Key);

                queue.Dequeue();
                if (queue.Count > 0 && card != null)
                {
                    card.DamagePopupText = queue.Peek();
                    card.DamagePopupVisible = false;
                    card.DamagePopupVisible = true;
                }
                else
                {
                    if (card != null)
                        card.DamagePopupVisible = false;
                    popupQueues.Remove(entry.Key);
                }
            }
        }

        private void QueueFlash(ICombatUnit? target, string kind)
        {
            if (target != null)
                pendingFlashes.Add(new PendingFlash(target.CombatInfo.CombatId, kind));
        }

        private void OnPopupShown(ICombatUnit target, PopupType type, string value)
        {
            switch (type)
            {
                case PopupType.Buff:
                case PopupType.Riposte:
                case PopupType.Guard:
                case PopupType.Cured:
                case PopupType.StressHeal:
                case PopupType.Unstun:
                case PopupType.Untagged:
                    QueueFlash(target, "Buff");
                    break;
                case PopupType.Debuff:
                case PopupType.DebuffResist:
                case PopupType.Bleed:
                case PopupType.BleedResist:
                case PopupType.Poison:
                case PopupType.PoisonResist:
                case PopupType.Stunned:
                case PopupType.StunResist:
                case PopupType.Tagged:
                case PopupType.MoveResist:
                case PopupType.Stress:
                    QueueFlash(target, "Damage");
                    break;
            }

            string? label = EffectPopupLabel(type, value);
            if (label != null)
                AddPopup(target, label, priority: 1);
        }

        private static string? EffectPopupLabel(PopupType type, string value)
        {
            switch (type)
            {
                case PopupType.Buff:
                    return "BUFF";
                case PopupType.Debuff:
                    return "DEBUFF";
                case PopupType.Bleed:
                    return "BLEED";
                case PopupType.Poison:
                    return "BLIGHT";
                case PopupType.Stunned:
                    return "STUN";
                case PopupType.Tagged:
                    return "MARK";
                case PopupType.Riposte:
                    return "RIPOSTE";
                case PopupType.Guard:
                    return "GUARD";
                case PopupType.Stress:
                    return value == null ? "STRESS" : "STRESS " + value;
                case PopupType.StressHeal:
                    return "STRESS HEAL";
                default:
                    return null;
            }
        }

        private void OnAiSkillPreviewed(string skillId)
        {
            var unit = controller.CurrentUnit;
            if (unit == null || string.IsNullOrEmpty(skillId))
                return;

            var skill = (unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                .FirstOrDefault(candidate => candidate.Id == skillId);
            if (skill == null)
                return;

            AiSkillPreview = new DuelSkillViewModel(skill.Id, skill.Id, Ui.SkillToneClassifier.Classify(skill))
            {
                Level = skill.Level,
                BaseInfo = Ui.SkillDetails.BuildBaseInfo(skill),
                EffectRows = Ui.SkillDetails.BuildEffectRows(skill),
                Details = Ui.SkillDetails.Build(skill),
            };
        }

        private void OnAiTargetPreviewed(int combatId)
        {
            var card = Heroes.FirstOrDefault(c => c.CombatId == combatId)
                ?? Monsters.FirstOrDefault(c => c.CombatId == combatId);
            if (card != null)
            {
                card.IsTarget = true;
                AiTargetPreview = card;
            }
        }

        private void OnRivalActionReceived(string payload)
        {
            ApplyRivalInput(payload);
        }

        private void RefreshUnits()
        {
            Heroes.Clear();
            foreach (var unit in heroOrder.OrderLeftToRight(controller.HeroParty))
                Heroes.Add(ToUnit(unit, isEnemy: false));
            Monsters.Clear();
            foreach (var unit in monsterOrder.OrderLeftToRight(controller.MonsterParty))
                Monsters.Add(ToUnit(unit, isEnemy: true));

            if (IsBuffTableVisible && BuffTarget != null)
                BuffTarget = Heroes.Concat(Monsters).FirstOrDefault(card => card.CombatId == BuffTarget.CombatId);

            var current = controller.CurrentUnit;
            MarkCurrent(Heroes, current);
            MarkCurrent(Monsters, current);
            RefreshTurnOrder(current);
        }

        private void RefreshTurnOrder(ICombatUnit? current)
        {
            TurnOrder.Clear();
            if (controller.BattleGround == null || current == null)
                return;

            int round = controller.BattleGround.Round.RoundNumber;
            if (round != _lastRound)
            {
                _lastRound = round;
                _roundStartOrder = controller.BattleGround.Round.OrderedUnits
                    .Select(unit => unit.CombatInfo.CombatId)
                    .ToList();
            }

            // The acting unit is removed from OrderedUnits when its turn is prepped, so make sure the
            // captured sequence always starts with it (keeps the strip from jumping mid-turn).
            var selected = controller.BattleGround.Round.SelectedUnit;
            if (selected != null && !_roundStartOrder.Contains(selected.CombatInfo.CombatId))
                _roundStartOrder.Insert(0, selected.CombatInfo.CombatId);

            int currentId = current.CombatInfo.CombatId;
            int currentIndex = _roundStartOrder.IndexOf(currentId);
            if (currentIndex < 0)
                currentIndex = 0;

            var unitsById = new Dictionary<int, ICombatUnit>();
            foreach (var unit in controller.HeroParty.Units.Concat(controller.MonsterParty.Units))
                unitsById[unit.CombatInfo.CombatId] = unit;

            int position = 0;
            foreach (var combatId in _roundStartOrder)
            {
                if (!unitsById.TryGetValue(combatId, out var unit))
                {
                    position++;
                    continue;
                }

                // Units that already moved this round or died are dropped from the strip; the
                // acting unit keeps its white frame until its turn resolves.
                if (unit.CombatInfo.IsDead || position < currentIndex)
                {
                    position++;
                    continue;
                }

                TurnOrder.Add(new DuelTurnEntryViewModel(
                    unit.Character.Name,
                    unit.Team == Team.Monsters,
                    (int)unit.Character.Speed)
                {
                    IsCurrent = combatId == currentId,
                });
                position++;
            }

            // Units that already moved this round (ordered before the current one) have no actions
            // left (gray pips); the acting unit and everyone still to come keep theirs (white pips).
            foreach (var card in Heroes.Concat(Monsters))
            {
                int index = _roundStartOrder.IndexOf(card.CombatId);
                bool dead = unitsById.TryGetValue(card.CombatId, out var unit) && unit.CombatInfo.IsDead;
                card.RemainingActions = dead || index < 0 || index < currentIndex ? 0 : 1;
            }
        }

        private static void MarkCurrent(ObservableCollection<DuelUnitViewModel> cards, ICombatUnit? current)
        {
            int currentId = current?.CombatInfo.CombatId ?? 0;
            foreach (var card in cards.Where(card => card.CombatId == currentId))
                card.IsCurrent = true;
        }

        private void RefreshSkills()
        {
            Skills.Clear();
            selectedSkill = null;
            selectedSkillId = null;
            OnPropertyChanged(nameof(SelectedSkill));

            if (controller.CurrentUnit == null)
                return;

            // Show the current unit's skills on every turn (local and opponent), so the bottom-left
            // strip stays consistent; they are only usable on the local turn.
            var unit = controller.CurrentUnit;
            bool localTurn = controller.IsLocalTurn;
            foreach (var skill in unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                Skills.Add(new DuelSkillViewModel(skill.Id, skill.Id, Ui.SkillToneClassifier.Classify(skill))
                {
                    IsUsable = localTurn && controller.IsSkillUsable(unit, skill),
                    Level = skill.Level,
                    BaseInfo = Ui.SkillDetails.BuildBaseInfo(skill),
                    EffectRows = Ui.SkillDetails.BuildEffectRows(skill),
                    Details = Ui.SkillDetails.Build(skill),
                });
        }

        private void RefreshStatus()
        {
            if (controller.IsFinished)
                Status = $"Battle finished. Round {controller.BattleGround!.Round.RoundNumber}.";
            else if (controller.IsLocalTurn)
                Status = $"Round {controller.BattleGround!.Round.RoundNumber} — your turn ({controller.CurrentUnit?.Character.Name}).";
            else
                Status = $"Round {controller.BattleGround!.Round.RoundNumber} — waiting for the opponent...";
        }

        private void RefreshLog()
        {
            while (controller.Events.Log.Count > Log.Count)
                Log.Add(controller.Events.Log[Log.Count]);
        }

        private void RefreshEvents()
        {
            int round = controller.BattleGround?.Round.RoundNumber ?? 1;
            Events.Round = round;
            Torch.Round = round;
        }

        private void RefreshActor()
        {
            var unit = controller.CurrentUnit;
            if (unit == null)
                return;

            var character = unit.Character;
            RaidHud.ApplyActor(
                character.Name,
                Ui.DisplayNames.Class(character.Class),
                character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>(),
                (int)character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                (int)character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue,
                (int)character.Stress.CurrentValue,
                (int)character.Speed,
                (int)character.MinDamage,
                (int)character.MaxDamage,
                (int)character.Accuracy,
                (int)(character.Crit * 100),
                (int)character.Dodge,
                (int)character.Protection);
            Quest.Goal = QuestText;
        }

        private void OpenStats(DuelUnitViewModel? unit)
        {
            if (unit == null)
                return;

            StatsTarget.Apply(unit);
            IsStatsVisible = true;
        }

        private void ToggleBuffTable(DuelUnitViewModel? unit)
        {
            if (unit == null)
                return;

            if (IsBuffTableVisible && ReferenceEquals(BuffTarget, unit))
                IsBuffTableVisible = false;
            else
            {
                BuffTarget = unit;
                IsBuffTableVisible = true;
            }
        }

        private void SelectSkill(DuelSkillViewModel? skill)
        {
            if (skill == null || !controller.IsLocalTurn)
                return;

            isMoveMode = false;
            foreach (var existing in Skills)
                existing.IsSelected = false;
            skill.IsSelected = true;
            selectedSkill = skill;
            selectedSkillId = skill.Id;
            OnPropertyChanged(nameof(SelectedSkill));

            ClearTargets();
            var unit = controller.CurrentUnit;
            var skillDef = unit?.Character.CurrentCombatSkills?.FirstOrDefault(s => s.Id == skill.Id);
            if (unit == null || skillDef == null)
                return;

            var targets = controller.GetAvailableTargets(unit, skillDef);
            foreach (var t in targets)
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == t.CombatInfo.CombatId);
                if (card != null)
                    card.IsTarget = true;
                card = Monsters.FirstOrDefault(h => h.CombatId == t.CombatInfo.CombatId);
                if (card != null)
                    card.IsTarget = true;
            }
        }

        private void SelectTarget(DuelUnitViewModel? unit)
        {
            if (unit == null || !controller.IsLocalTurn)
                return;

            if (isMoveMode)
            {
                int actorRank = controller.CurrentUnit?.Rank ?? 0;
                if (Math.Abs(unit.Rank - actorRank) != 1)
                    return;

                string? actor = controller.CurrentUnit?.Character.Name;
                var movePayload = controller.ExecuteLocalMove(unit.Rank);
                isMoveMode = false;
                if (movePayload == null)
                    return;
                LogAction(actor, movePayload);
                rivalLink.SendLocalAction(movePayload);
                Refresh();
                return;
            }

            if (selectedSkill == null || !unit.IsTarget)
                return;

            string? actorName = controller.CurrentUnit?.Character.Name;
            var payload = controller.ExecuteLocalSkill(selectedSkillId!, unit.CombatId);
            if (payload == null)
                return;

            LogAction(actorName, payload);
            rivalLink.SendLocalAction(payload);
            Refresh();
        }

        private void SelectMove()
        {
            if (!controller.IsLocalTurn || controller.CurrentUnit == null)
                return;

            selectedSkill = null;
            selectedSkillId = null;
            isMoveMode = true;
            OnPropertyChanged(nameof(SelectedSkill));
            foreach (var existing in Skills)
                existing.IsSelected = false;
            ClearTargets();

            var unit = controller.CurrentUnit;
            var allies = unit.Team == Team.Heroes ? Heroes : Monsters;
            foreach (var ally in allies)
            {
                if (Math.Abs(ally.Rank - unit.Rank) == 1)
                    ally.IsTarget = true;
            }
        }

        private void Pass()
        {
            if (!controller.IsLocalTurn)
                return;

            string? actor = controller.CurrentUnit?.Character.Name;
            var payload = controller.ExecuteLocalPass();
            if (payload == null)
                return;

            LogAction(actor, payload);
            rivalLink.SendLocalAction(payload);
            Refresh();
        }

        private void LogAction(string? actorName, string payload)
        {
            var parts = payload.Split('|');
            if (parts.Length < 1 || actorName == null)
                return;

            if (parts[0] == DuelPayload.Pass)
            {
                controller.Events.Log.Add($"{actorName} passes.");
                return;
            }
            if (parts[0] == DuelPayload.Move)
            {
                controller.Events.Log.Add(parts.Length == 2 ? $"{actorName} moves to rank {parts[1]}." : $"{actorName} moves.");
                return;
            }
            LogSkillResult(actorName, parts[0]);
        }

        private void LogSkillResult(string actorName, string skillId)
        {
            if (controller.Solver == null)
                return;

            var result = controller.Solver.SkillResult;
            var skill = result.Skill;
            int missChance = skill != null && skill.Accuracy > 0 ? 100 - (int)(skill.Accuracy * 100) : 0;
            int critChance = skill != null ? (int)(skill.CritMod * 100) : 0;
            foreach (var entry in result.SkillEntries)
            {
                string target = entry.Target?.Character.Name ?? "the void";
                switch (entry.Type)
                {
                    case SkillResultType.Miss:
                        controller.Events.Log.Add($"{actorName}: {skillId} misses {target} ({missChance}% chance).");
                        AddPopup(entry.Target, "MISS");
                        break;
                    case SkillResultType.Dodge:
                        controller.Events.Log.Add($"{actorName}: {skillId} is dodged by {target}.");
                        AddPopup(entry.Target, "DODGE");
                        break;
                    case SkillResultType.Hit:
                        controller.Events.Log.Add(entry.IsZeroed
                            ? $"{actorName}: {skillId} slays {target} for {entry.Amount} damage!"
                            : $"{actorName}: {skillId} hits {target} for {entry.Amount} damage.");
                        AddPopup(entry.Target, entry.IsZeroed ? entry.Amount + "!" : entry.Amount.ToString());
                        QueueFlash(entry.Target, "Damage");
                        break;
                    case SkillResultType.Crit:
                        controller.Events.Log.Add(entry.IsZeroed
                            ? $"{actorName}: {skillId} CRITS and slays {target} for {entry.Amount} damage! ({critChance}% chance)"
                            : $"{actorName}: {skillId} CRITS {target} for {entry.Amount} damage! ({critChance}% chance)");
                        AddPopup(entry.Target, "CRIT!\n" + entry.Amount);
                        QueueFlash(entry.Target, "Damage");
                        break;
                    case SkillResultType.Heal:
                        controller.Events.Log.Add($"{actorName}: {skillId} heals {target} for {entry.Amount}.");
                        AddPopup(entry.Target, "+" + entry.Amount);
                        QueueFlash(entry.Target, "Heal");
                        break;
                    case SkillResultType.CritHeal:
                        controller.Events.Log.Add($"{actorName}: {skillId} critically heals {target} for {entry.Amount}!");
                        AddPopup(entry.Target, "+" + entry.Amount + "!");
                        QueueFlash(entry.Target, "Heal");
                        break;
                    default:
                        controller.Events.Log.Add($"{actorName}: {skillId} affects {target}.");
                        break;
                }
            }
        }

        private void AddPopup(ICombatUnit? target, string text, int priority = 0)
        {
            if (target == null)
                return;
            pendingPopups.Add(new PendingPopup(target.CombatInfo.CombatId, text, priority));
        }

        private void ClearTargets()
        {
            foreach (var hero in Heroes)
                hero.IsTarget = false;
            foreach (var monster in Monsters)
                monster.IsTarget = false;
        }

        /// <summary>Whether the hovered unit can show the attack arrow (a valid skill/move target
        /// during the local turn, other than the acting unit itself).</summary>
        /// <param name="target">The hovered unit card.</param>
        /// <returns>True when the arrow may be drawn.</returns>
        public bool CanShowArrow(DuelUnitViewModel target)
        {
            var current = controller.CurrentUnit;
            return controller.IsLocalTurn
                && current != null
                && target != null
                && target.CombatId != current.CombatInfo.CombatId
                && target.IsTarget
                && (selectedSkill != null || isMoveMode);
        }

        private DuelUnitViewModel ToUnit(ICombatUnit unit, bool isEnemy)
        {
            var character = unit.Character;
            var hp = character.GetPairedAttribute(AttributeType.HitPoints);
            return new DuelUnitViewModel(
                unit.CombatInfo.CombatId,
                unit.Rank,
                character.Name,
                Ui.DisplayNames.Class(character.Class))
            {
                IsEnemy = isEnemy,
                HpCurrent = (int)hp.CurrentValue,
                HpMax = (int)hp.ModifiedValue,
                Stress = (int)character.Stress.CurrentValue,
                Speed = (int)character.Speed,
                Damage = (int)character.MinDamage + " - " + (int)character.MaxDamage,
                Accuracy = (int)character.Accuracy,
                Crit = (int)(character.Crit * 100),
                Dodge = (int)character.Dodge,
                Protection = (int)character.Protection,
                ResistStun = (int)(character.GetSingleAttribute(AttributeType.Stun).ModifiedValue * 100),
                ResistBlight = (int)(character.GetSingleAttribute(AttributeType.Poison).ModifiedValue * 100),
                ResistBleed = (int)(character.GetSingleAttribute(AttributeType.Bleed).ModifiedValue * 100),
                ResistDebuff = (int)(character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue * 100),
                ResistMove = (int)(character.GetSingleAttribute(AttributeType.Move).ModifiedValue * 100),
                ResistDisease = (int)(character.GetSingleAttribute(AttributeType.Disease).ModifiedValue * 100),
                ResistDeathBlow = (int)(character.GetSingleAttribute(AttributeType.DeathBlow).ModifiedValue * 100),
                ResistTrap = (int)(character.GetSingleAttribute(AttributeType.Trap).ModifiedValue * 100),
                Debuffs = BuildDebuffs(character),
                Buffs = BuildBuffs(character),
                AllSkills = character is Hero hero
                    ? string.Join(", ", hero.HeroClass.CombatSkills.Select(skill => skill.Id))
                    : string.Empty,
                QuirksText = character is Hero heroWithQuirks && heroWithQuirks.Quirks.Count > 0
                    ? string.Join(", ", heroWithQuirks.Quirks.Select(quirkId =>
                        (QuirkCatalog.Get(quirkId)?.IsPositive == true ? "+" : "-") + quirkId))
                    : "none",
            };
        }

        private static List<BuffRowViewModel> BuildBuffs(ICharacter character)
        {
            return BuildBuffRows(character, positive: true);
        }

        private static List<BuffRowViewModel> BuildDebuffs(ICharacter character)
        {
            return BuildBuffRows(character, positive: false);
        }

        private static List<BuffRowViewModel> BuildBuffRows(ICharacter character, bool positive)
        {
            var rows = new List<BuffRowViewModel>();
            var source = character as Character;
            if (source != null)
            {
                foreach (var buffInfo in source.BuffInfos)
                {
                    var buff = buffInfo.Buff;
                    bool isPositive = buff.IsPositive();
                    if (isPositive != positive)
                        continue;

                    rows.Add(new BuffRowViewModel(
                        Ui.BuffDetails.FormatName(buff),
                        Ui.BuffDetails.FormatDuration(buffInfo),
                        Ui.BuffDetails.FormatDescription(buff),
                        isPositive ? "Buff" : "Debuff"));
                }
            }

            AppendStatusRows(character, positive, rows);
            return rows;
        }

        private static void AppendStatusRows(ICharacter character, bool positive, List<BuffRowViewModel> rows)
        {
            var bleeding = character.GetStatusEffect(StatusType.Bleeding) as DamageOverTimeStatusEffect;
            if (bleeding != null && bleeding.IsApplied)
                AddStatusRow(rows, positive, false, "Bleeding", bleeding.ExpirationTime + " rounds",
                    bleeding.CurrentTickDamage + " dmg per round");

            var poison = character.GetStatusEffect(StatusType.Poison) as DamageOverTimeStatusEffect;
            if (poison != null && poison.IsApplied)
                AddStatusRow(rows, positive, false, "Blight", poison.ExpirationTime + " rounds",
                    poison.CurrentTickDamage + " dmg per round");

            var stun = character.GetStatusEffect(StatusType.Stun) as IStunStatusEffect;
            if (stun != null && stun.IsApplied)
                AddStatusRow(rows, positive, false, "Stunned", "-", "Cannot act this round");

            var mark = character.GetStatusEffect(StatusType.Marked) as IMarkStatusEffect;
            if (mark != null && mark.IsApplied)
                AddStatusRow(rows, positive, false, "Marked", mark.MarkDuration + " rounds", "Vulnerable to attacks");

            var riposte = character.GetStatusEffect(StatusType.Riposte) as IRiposteStatusEffect;
            if (riposte != null && riposte.IsApplied)
                AddStatusRow(rows, positive, true, "Riposte", riposte.RiposteDuration + " rounds", "Counterattacks when hit");

            var guarded = character.GetStatusEffect(StatusType.Guarded) as IGuardedStatusEffect;
            if (guarded != null && guarded.IsApplied)
                AddStatusRow(rows, positive, true, "Guarded", guarded.GuardDuration + " rounds",
                    "Protected by " + (guarded.Guard?.Character.Name ?? "?"));

            var guard = character.GetStatusEffect(StatusType.Guard) as IGuardStatusEffect;
            if (guard != null && guard.IsApplied)
                AddStatusRow(rows, positive, true, "Guarding", "-", "Protects " + guard.Targets.Count + " allies");
        }

        private static void AddStatusRow(List<BuffRowViewModel> rows, bool positive, bool rowIsPositive, string name, string duration, string description)
        {
            if (rowIsPositive == positive)
                rows.Add(new BuffRowViewModel(name, duration, description, rowIsPositive ? "Buff" : "Debuff"));
        }
    }
}