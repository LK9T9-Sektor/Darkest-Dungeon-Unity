using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        /// <summary>Gets or sets the status line (round / turn / result).</summary>
        [ObservableProperty]
        private string _status = string.Empty;

        /// <summary>Gets the command that selects a skill.</summary>
        public IRelayCommand<DuelSkillViewModel> SelectSkillCommand { get; }

        /// <summary>Gets the command that targets and executes.</summary>
        public IRelayCommand<DuelUnitViewModel> TargetCommand { get; }

        /// <summary>Gets the command that abandons the duel and returns to the main menu.</summary>
        public IRelayCommand LeaveCommand { get; }

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
            LeaveCommand = new RelayCommand(Leave);
            controller.Events.StateChanged += Refresh;
            rivalLink.RivalActionReceived += OnRivalActionReceived;
            rivalLink.Attach(controller);
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

        private void SelectSkill(DuelSkillViewModel? skill)
        {
            if (skill == null || !controller.IsLocalTurn)
                return;

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
            if (unit == null || selectedSkill == null || !controller.IsLocalTurn)
                return;

            var payload = controller.ExecuteLocalSkill(selectedSkillId!, unit.CombatId);
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
            return new DuelUnitViewModel(
                unit.CombatInfo.CombatId,
                character.Name,
                character.Class)
            {
                IsEnemy = isEnemy,
                Hp = (int)character.HealthRatio * 100,
                HpMax = 100,
                Stress = (int)character.Stress.CurrentValue,
            };
        }
    }
}
