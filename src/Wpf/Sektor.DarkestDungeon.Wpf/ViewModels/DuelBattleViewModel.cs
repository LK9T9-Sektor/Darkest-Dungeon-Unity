using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Character;
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

            public PendingPopup(int combatId, string text)
            {
                CombatId = combatId;
                Text = text;
            }
        }

        private readonly DuelController controller;
        private readonly IDuelRivalLink rivalLink;
        private readonly Action onLeave;
        private readonly DispatcherTimer popupTimer;
        private readonly System.Collections.Generic.List<PendingPopup> pendingPopups =
            new System.Collections.Generic.List<PendingPopup>();
        private string? selectedSkillId;
        private DuelSkillViewModel? selectedSkill;
        private bool isMoveMode;

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

        /// <summary>Gets the team of the unit whose turn is being played.</summary>
        public Team CurrentActorTeam
        {
            get { return controller.CurrentUnit?.Team ?? Team.Heroes; }
        }

        /// <summary>Gets a value indicating whether it is the local player's turn.</summary>
        public bool IsLocalTurn { get { return controller.IsLocalTurn; } }

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
            controller.Events.StateChanged += Refresh;
            rivalLink.RivalActionReceived += OnRivalActionReceived;
            rivalLink.Attach(controller);
            Quest.Title = "Duel";
            popupTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            popupTimer.Tick += (s, e) => ClearPopups();
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
            rivalLink.RivalActionReceived -= OnRivalActionReceived;
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
        }

        private void ClearPopups()
        {
            foreach (var card in Heroes.Concat(Monsters))
            {
                card.DamagePopupVisible = false;
                card.DamagePopupText = string.Empty;
            }
        }

        private void ApplyPopups()
        {
            if (pendingPopups.Count == 0)
                return;

            foreach (var popup in pendingPopups)
            {
                var card = Heroes.FirstOrDefault(h => h.CombatId == popup.CombatId)
                    ?? Monsters.FirstOrDefault(m => m.CombatId == popup.CombatId);
                if (card == null)
                    continue;
                card.DamagePopupText = popup.Text;
                card.DamagePopupVisible = true;
            }
            pendingPopups.Clear();
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

            if (!controller.IsLocalTurn || controller.CurrentUnit == null)
                return;

            var unit = controller.CurrentUnit;
            foreach (var skill in unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                Skills.Add(new DuelSkillViewModel(skill.Id, skill.Id)
                {
                    IsUsable = controller.IsSkillUsable(unit, skill),
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
                        break;
                    case SkillResultType.Crit:
                        controller.Events.Log.Add(entry.IsZeroed
                            ? $"{actorName}: {skillId} CRITS and slays {target} for {entry.Amount} damage! ({critChance}% chance)"
                            : $"{actorName}: {skillId} CRITS {target} for {entry.Amount} damage! ({critChance}% chance)");
                        AddPopup(entry.Target, "CRIT!\n" + entry.Amount);
                        break;
                    case SkillResultType.Heal:
                        controller.Events.Log.Add($"{actorName}: {skillId} heals {target} for {entry.Amount}.");
                        AddPopup(entry.Target, "+" + entry.Amount);
                        break;
                    case SkillResultType.CritHeal:
                        controller.Events.Log.Add($"{actorName}: {skillId} critically heals {target} for {entry.Amount}!");
                        AddPopup(entry.Target, "+" + entry.Amount + "!");
                        break;
                    default:
                        controller.Events.Log.Add($"{actorName}: {skillId} affects {target}.");
                        break;
                }
            }
        }

        private void AddPopup(ICombatUnit? target, string text)
        {
            if (target == null)
                return;
            pendingPopups.Add(new PendingPopup(target.CombatInfo.CombatId, text));
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
                StatusEffects = BuildStatusEffects(character),
                AllSkills = character is Hero hero
                    ? string.Join(", ", hero.HeroClass.CombatSkills.Select(skill => skill.Id))
                    : string.Empty,
                QuirksText = character is Hero heroWithQuirks && heroWithQuirks.Quirks.Count > 0
                    ? string.Join(", ", heroWithQuirks.Quirks.Select(quirkId =>
                        (QuirkCatalog.Get(quirkId)?.IsPositive == true ? "+" : "-") + quirkId))
                    : "none",
            };
        }

        private static List<string> BuildStatusEffects(ICharacter character)
        {
            var source = character as Character;
            if (source == null || source.BuffInfos.Count == 0)
                return new List<string>();

            var labels = new List<string>(source.BuffInfos.Count);
            foreach (var buffInfo in source.BuffInfos)
            {
                string id = buffInfo.Buff.Id;
                if (string.IsNullOrEmpty(id))
                    id = buffInfo.Buff.AttributeType.ToString();
                labels.Add(buffInfo.DurationType == BuffDurationType.Round && buffInfo.Duration > 0
                    ? id + " x" + buffInfo.Duration
                    : id);
            }
            return labels;
        }
    }
}