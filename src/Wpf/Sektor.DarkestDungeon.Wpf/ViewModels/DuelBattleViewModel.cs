using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Live snapshot of a duel: units, skills, status and click inputs wired to the core controller.</summary>
    public partial class DuelBattleViewModel : ObservableObject, IPumpable
    {
        private readonly DuelController controller;
        private readonly IDuelRivalLink rivalLink;
        private readonly Action onLeave;
        private string? selectedSkillId;
        private DuelSkillViewModel? selectedSkill;
        private bool isMoveMode;

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

        /// <summary>Gets or sets the unit shown in the hover tooltip.</summary>
        [ObservableProperty]
        private DuelUnitViewModel? _tooltipTarget;

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

        /// <summary>Gets the command that shows a unit in the hover tooltip.</summary>
        public IRelayCommand<DuelUnitViewModel> HoverCommand { get; }

        /// <summary>Gets the command that hides the hover tooltip.</summary>
        public IRelayCommand UnhoverCommand { get; }

        /// <summary>Gets the command that opens the stats sheet for the given unit.</summary>
        public IRelayCommand<DuelUnitViewModel> OpenStatsCommand { get; }

        /// <summary>Gets the command that closes the stats sheet.</summary>
        public IRelayCommand CloseStatsCommand { get; }

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
            HoverCommand = new RelayCommand<DuelUnitViewModel>(Hover);
            UnhoverCommand = new RelayCommand(Unhover);
            OpenStatsCommand = new RelayCommand<DuelUnitViewModel>(OpenStats);
            CloseStatsCommand = new RelayCommand(() => IsStatsVisible = false);
            controller.Events.StateChanged += Refresh;
            rivalLink.RivalActionReceived += OnRivalActionReceived;
            rivalLink.Attach(controller);
            Quest.Title = "Duel";
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
            controller.ApplyRemoteSkill(payload);
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
        }

        private void OnRivalActionReceived(string payload)
        {
            ApplyRivalInput(payload);
        }

        private void RefreshUnits()
        {
            Heroes.Clear();
            foreach (var unit in controller.HeroParty.Units)
                Heroes.Add(ToUnit(unit, isEnemy: false));
            Monsters.Clear();
            foreach (var unit in controller.MonsterParty.Units)
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

            int currentId = current.CombatInfo.CombatId;
            foreach (var unit in controller.BattleGround.Round.OrderedUnits)
            {
                var entry = new DuelTurnEntryViewModel(unit.Character.Name, unit.Team == Team.Monsters);
                entry.IsCurrent = unit.CombatInfo.CombatId == currentId;
                TurnOrder.Add(entry);
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

            if (!controller.IsLocalTurn || controller.CurrentUnit == null)
                return;

            var unit = controller.CurrentUnit;
            foreach (var skill in unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                Skills.Add(new DuelSkillViewModel(skill.Id, skill.Id) { IsUsable = controller.IsSkillUsable(unit, skill) });
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
            Events.Round = controller.BattleGround?.Round.RoundNumber ?? 1;
        }

        private void RefreshActor()
        {
            var unit = controller.CurrentUnit;
            if (unit == null)
                return;

            var character = unit.Character;
            RaidHud.ApplyActor(
                character.Name,
                character.Class,
                character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>(),
                (int)character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                (int)character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue,
                (int)character.Stress.CurrentValue);
            Quest.Goal = Status;
        }

        private void Hover(DuelUnitViewModel? unit)
        {
            if (unit == null)
                return;

            unit.IsSelected = true;
            TooltipTarget = unit;
        }

        private void Unhover()
        {
            if (TooltipTarget != null)
                TooltipTarget.IsSelected = false;
            TooltipTarget = null;
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
                var movePayload = controller.ExecuteLocalMove(unit.Rank);
                isMoveMode = false;
                if (movePayload == null)
                    return;
                rivalLink.SendLocalAction(movePayload);
                Refresh();
                return;
            }

            if (selectedSkill == null)
                return;

            var payload = controller.ExecuteLocalSkill(selectedSkillId!, unit.CombatId);
            if (payload == null)
                return;

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

            var payload = controller.ExecuteLocalPass();
            if (payload == null)
                return;

            rivalLink.SendLocalAction(payload);
            Refresh();
        }

        private void ClearTargets()
        {
            foreach (var hero in Heroes)
                hero.IsTarget = false;
            foreach (var monster in Monsters)
                monster.IsTarget = false;
        }

        private DuelUnitViewModel ToUnit(ICombatUnit unit, bool isEnemy)
        {
            var character = unit.Character;
            var hp = character.GetPairedAttribute(AttributeType.HitPoints);
            return new DuelUnitViewModel(
                unit.CombatInfo.CombatId,
                unit.Rank,
                character.Name,
                character.Class)
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
            };
        }
    }
}