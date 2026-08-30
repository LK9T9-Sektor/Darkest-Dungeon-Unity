using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>
    /// Resolves the heal branch of a skill: base heal amount, crit heal. Pure computation — the
    /// caller applies effects and the crit-heal stress relief afterwards.
    /// </summary>
    public class HealResolver
    {
        /// <summary>Resolves a heal against a target.</summary>
        /// <param name="performer">The performing character.</param>
        /// <param name="target">The target character.</param>
        /// <param name="targetUnit">The target unit (for the result entry).</param>
        /// <param name="skill">The skill.</param>
        /// <returns>The resolved heal entry.</returns>
        public SkillResultEntry Resolve(ICharacter performer, ICharacter target, ICombatUnit targetUnit, CombatSkill skill)
        {
            float initialHeal = RandomSolver.Next(skill.Heal.MinAmount, skill.Heal.MaxAmount + 1) *
                (1 + performer.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

            if (skill.IsCritValid)
            {
                float critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod / 100;
                if (RandomSolver.CheckSuccess(critChance))
                {
                    int critHeal = target.Heal(initialHeal * BattleConstants.CritMultiplier, true);
                    return new SkillResultEntry(targetUnit, critHeal, SkillResultType.CritHeal);
                }
            }

            int heal = target.Heal(initialHeal, true);
            return new SkillResultEntry(targetUnit, heal, SkillResultType.Heal);
        }
    }
}