using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Abstract base for target selection desires.</summary>
    public abstract class TargetSelectionDesire : IProportionValue
    {
        /// <summary>Gets or sets the target desire type.</summary>
        public TargetDesireType Type { get; protected set; }

        /// <summary>Gets or sets the proportional chance weight.</summary>
        public int Chance { get; set; }

        /// <summary>Gets the target selection parameters.</summary>
        protected Dictionary<TargetSelectParameter, bool?> Parameters { get; private set; }

        private string SpecificCombatSkillId { get; set; }
        private bool IsEnemyTargetDesire { get; set; }
        private bool IsFriendlyTargetDesire { get; set; }

        /// <summary>Initializes a new instance of the <see cref="TargetSelectionDesire"/> class.</summary>
        protected TargetSelectionDesire()
        {
            Parameters = new Dictionary<TargetSelectParameter, bool?>();
            foreach (TargetSelectParameter selectionAttribute in System.Enum.GetValues(typeof(TargetSelectParameter)))
                Parameters.Add(selectionAttribute, null);
        }

        /// <summary>Selects targets for the given decision.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="decision">The brain decision.</param>
        /// <returns>True if targets were selected.</returns>
        public virtual bool SelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
                return false;

            if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
                return false;

            if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation ||
                decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
                return false;

            return ChooseTargets(FilterTargets(performer, decision.TargetInfo.Targets), decision);
        }

        /// <summary>Filters possible targets by the configured parameters.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="possibleTargets">The list of possible targets.</param>
        /// <returns>The filtered target list.</returns>
        protected virtual List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = new List<ICombatUnit>(possibleTargets);

            if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
                if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                    availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

            if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
                if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                    availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

            if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
                if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                    availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

            if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
                if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                    availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

            if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
                if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                    availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

            return availableTargets;
        }

        /// <summary>Chooses final targets from the available list.</summary>
        /// <param name="availableTargets">The available targets.</param>
        /// <param name="decision">The brain decision.</param>
        /// <returns>True if targets were chosen.</returns>
        protected virtual bool ChooseTargets(List<ICombatUnit> availableTargets, MonsterBrainDecision decision)
        {
            if (availableTargets.Count > 0)
            {
                decision.TargetInfo.Targets.Clear();

                if (decision.SelectedSkill.TargetRanks.IsMultitarget)
                {
                    decision.TargetInfo.Targets.AddRange(availableTargets);
                    return true;
                }
                else
                {
                    int index = RandomSolver.Next(availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[index]);
                    availableTargets.RemoveAt(index);

                    if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                        RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                    {
                        int sideTargetIndex = RandomSolver.Next(availableTargets.Count);
                        decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                        return true;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>Populates parameters from a data set dictionary.</summary>
        /// <param name="dataSet">The data set to process.</param>
        protected virtual void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
                ProcessBaseDataToken(token);
        }

        /// <summary>Processes a single key-value token from the data set.</summary>
        /// <param name="token">The key-value pair to process.</param>
        protected void ProcessBaseDataToken(KeyValuePair<string, object> token)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)token.Value;
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)token.Value;
                    break;
                case "is_exclusive_desire":
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)token.Value;
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)token.Value;
                    break;
            }
        }
    }
}
