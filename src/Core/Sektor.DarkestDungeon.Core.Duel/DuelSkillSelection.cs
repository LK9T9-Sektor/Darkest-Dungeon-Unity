using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Random skill selection desire for duels, mirroring Darkest Dungeon's "random_skill".
    /// Runs through the base <see cref="SkillSelectionDesire.SelectSkill"/> flow (deterministic
    /// <c>RandomSolver</c>, weighted target desires from the injected brain), but picks from the
    /// unit's <em>selected</em> combat skills.
    /// </summary>
    public class DuelSkillSelection : SkillSelectionDesire
    {
        private readonly MonsterBrain brain;

        /// <summary>Initializes a new instance of the <see cref="DuelSkillSelection"/> class.</summary>
        /// <param name="brain">The brain providing the target desire set.</param>
        /// <param name="chance">The proportional selection chance.</param>
        public DuelSkillSelection(MonsterBrain brain, int chance)
        {
            this.brain = brain;
            Chance = chance;
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