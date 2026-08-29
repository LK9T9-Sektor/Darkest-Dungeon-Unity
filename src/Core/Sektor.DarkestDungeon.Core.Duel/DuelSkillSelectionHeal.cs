using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Heal skill selection desire for duels, mirroring Darkest Dungeon's "heal_skill": the unit
    /// heals when an ally is below the HP ratio threshold. Runs through the base
    /// <see cref="SkillSelectionDesire.SelectSkill"/> flow with only the health target desire.
    /// </summary>
    public class DuelSkillSelectionHeal : SkillSelectionDesire
    {
        private readonly MonsterBrain brain;
        private readonly float hpRatioThreshold;

        /// <summary>Initializes a new instance of the <see cref="DuelSkillSelectionHeal"/> class.</summary>
        /// <param name="brain">The brain providing the target desire set.</param>
        /// <param name="hpRatioThreshold">The ally HP ratio below which healing is considered.</param>
        /// <param name="chance">The proportional selection chance.</param>
        public DuelSkillSelectionHeal(MonsterBrain brain, float hpRatioThreshold, int chance)
        {
            this.brain = brain;
            this.hpRatioThreshold = hpRatioThreshold;
            Chance = chance;
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            return skill.Heal != null;
        }

        /// <inheritdoc/>
        protected override bool IsValidTarget(ICombatUnit target)
        {
            return target.Character.HealthRatio < hpRatioThreshold;
        }

        /// <inheritdoc/>
        protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return desire.Type == TargetDesireType.Health;
        }

        /// <inheritdoc/>
        protected override List<CombatSkill> GetMonsterCombatSkills(ICombatUnit performer)
        {
            return performer.Character.CurrentCombatSkills ?? new List<CombatSkill>();
        }

        /// <inheritdoc/>
        protected override MonsterBrain GetMonsterBrain(ICombatUnit performer)
        {
            return brain;
        }
    }
}