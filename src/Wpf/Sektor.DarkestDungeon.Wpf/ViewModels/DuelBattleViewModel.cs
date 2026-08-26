using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Live snapshot of a duel: units, skills, status and click inputs wired to the core controller.</summary>
    public partial class DuelBattleViewModel : ObservableObject
    {
        private readonly DuelController controller;
        private readonly Action<string, string> sendInput;
        private string? selectedSkillId;
        private DuelSkillViewModel? selectedSkill;

        /// <summary>Gets the hero unit cards.</summary>
        public ObservableCollection<DuelUnitViewModel> Heroes { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the monster unit cards.</summary>
        public ObservableCollection<DuelUnitViewModel> Monsters { get; } = new ObservableCollection<DuelUnitViewModel>();

        /// <summary>Gets the skill buttons of the acting unit.</summary>
        public ObservableCollection<DuelSkillViewModel> Skills { get; } = new ObservableCollection<DuelSkillViewModel>();

        /// <summary>Gets the battle log lines.</summary>
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        /// <summary>Gets or sets the status line (round / turn / result).</summary>
        [ObservableProperty]
        private string _status = string.Empty;

        /// <summary>Gets the command that selects a skill.</summary>
        public IRelayCommand<DuelSkillViewModel> SelectSkillCommand { get; }

        /// <summary>Gets the command that targets and executes.</summary>
        public IRelayCommand<DuelUnitViewModel> TargetCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelBattleViewModel"/> class.</summary>
        /// <param name="controller">The duel controller.</param>
        /// <param name="sendInput">Sends an input message (method, payload).</param>
        public DuelBattleViewModel(DuelController controller, Action<string, string> sendInput)
        {
            this.controller = controller;
            this.sendInput = sendInput;
            SelectSkillCommand = new RelayCommand<DuelSkillViewModel>(SelectSkill);
            TargetCommand = new RelayCommand<DuelUnitViewModel>(SelectTarget);
            controller.Events.StateChanged += Refresh;
            Refresh();
        }

        /// <summary>Applies a rival input and refreshes the snapshot.</summary>
        /// <param name="payload">The input payload.</param>
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

        private void RefreshUnits()
        {
            Heroes.Clear();
            foreach (var unit in controller.HeroParty.Units)
                Heroes.Add(ToUnit(unit));
            Monsters.Clear();
            foreach (var unit in controller.MonsterParty.Units)
                Monsters.Add(ToUnit(unit));

            var current = controller.CurrentUnit;
            var target = Heroes.FirstOrDefault(h => h.CombatId == (current?.CombatInfo.CombatId ?? 0));
            var monsterTarget = Monsters.FirstOrDefault(h => h.CombatId == (current?.CombatInfo.CombatId ?? 0));
            if (target != null)
                target.IsCurrent = true;
            if (monsterTarget != null)
                monsterTarget.IsCurrent = true;
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

            sendInput("hero_skill", payload);
            Refresh();
        }

        private void ClearTargets()
        {
            foreach (var hero in Heroes)
                hero.IsTarget = false;
            foreach (var monster in Monsters)
                monster.IsTarget = false;
        }

        private DuelUnitViewModel ToUnit(ICombatUnit unit)
        {
            var character = unit.Character;
            return new DuelUnitViewModel(
                unit.CombatInfo.CombatId,
                character.Name,
                character.Class)
            {
                Hp = (int)character.HealthRatio * 100,
                HpMax = 100,
                Stress = (int)character.Stress.CurrentValue,
            };
        }
    }
}