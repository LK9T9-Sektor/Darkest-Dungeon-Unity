using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Skill selection desire for duels: picks a random usable skill from the unit's selected
    /// combat skills. Selection uses a client-local RNG so the deterministic simulation
    /// (<see cref="Sektor.DarkestDungeon.Core.Combat.Mechanics.RandomSolver"/>) stays in lockstep.
    /// </summary>
    public class DuelSkillSelection : SkillSelectionDesire
    {
        private readonly Random random;

        /// <summary>Initializes a new instance of the <see cref="DuelSkillSelection"/> class.</summary>
        /// <param name="random">The client-local random used for selection.</param>
        public DuelSkillSelection(Random random)
        {
            this.random = random;
        }

        /// <summary>Selects a random usable skill and fills the decision's target pool.</summary>
        /// <param name="performer">The acting unit.</param>
        /// <param name="decision">The decision to populate.</param>
        /// <param name="battleContext">The battle context.</param>
        /// <returns>True when a usable skill with at least one target was selected.</returns>
        public new bool SelectSkill(ICombatUnit performer, MonsterBrainDecision decision, IBattleContext battleContext)
        {
            var legal = GetMonsterCombatSkills(performer).FindAll(skill => battleContext.IsSkillUsable(performer, skill));
            if (legal.Count == 0)
                return false;

            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = legal[random.Next(legal.Count)];
            decision.TargetInfo.Targets = battleContext.GetSkillAvailableTargets(performer, decision.SelectedSkill);
            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.SkillTargetType;
            return decision.TargetInfo.Targets.Count > 0;
        }

        /// <inheritdoc/>
        protected override List<CombatSkill> GetMonsterCombatSkills(ICombatUnit performer)
        {
            return performer.Character.CurrentCombatSkills ?? new List<CombatSkill>();
        }
    }
}