using System;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>
    /// Resolves the damage branch of a skill: hit chance, damage amount, crit. Pure computation —
    /// no battle-context side effects; the caller applies effects and stress afterwards.
    /// </summary>
    public class DamageResolver
    {
        /// <summary>Resolves a hit against a target.</summary>
        /// <param name="performer">The performing character.</param>
        /// <param name="target">The target character.</param>
        /// <param name="targetUnit">The target unit (for the result entry).</param>
        /// <param name="skill">The skill.</param>
        /// <returns>The resolved damage entry; a Miss/Dodge entry when the attack does not land.</returns>
        public SkillResultEntry Resolve(ICharacter performer, ICharacter target, ICombatUnit targetUnit, CombatSkill skill)
        {
            float accuracy = skill.Accuracy + performer.Accuracy;
            float hitChance = Clamp(accuracy - target.Dodge, 0, BattleConstants.MaxChance);
            float roll = (float)RandomSolver.NextDouble();
            if (target.BattleModifiers != null && target.BattleModifiers.CanBeHit == false)
                roll = float.MaxValue;

            if (roll > hitChance)
            {
                bool canMiss = !(skill.CanMiss == false
                    || (target.BattleModifiers != null && target.BattleModifiers.CanBeMissed == false));
                if (canMiss)
                {
                    SkillResultType missType = roll > Math.Min(accuracy, BattleConstants.MaxChance)
                        ? SkillResultType.Miss
                        : SkillResultType.Dodge;
                    return new SkillResultEntry(targetUnit, missType);
                }
            }

            float initialDamage = !performer.IsMonster
                ? Lerp(performer.MinDamage, performer.MaxDamage, (float)RandomSolver.NextDouble()) * (1 + skill.DamageMod)
                : Lerp(skill.DamageMin, skill.DamageMax, (float)RandomSolver.NextDouble()) * performer.DamageMod;

            int damage = CeilToInt(initialDamage * (1 - target.Protection));
            if (damage < 0)
                damage = 0;

            if (target.BattleModifiers != null && target.BattleModifiers.CanBeDamagedDirectly == false)
                damage = 0;

            if (skill.IsCritValid)
            {
                float critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod;
                if (RandomSolver.CheckSuccess(critChance))
                {
                    int critDamage = target.TakeDamage(damage * BattleConstants.CritMultiplier);
                    return new SkillResultEntry(targetUnit, critDamage, target.HasZeroHealth, SkillResultType.Crit);
                }
            }

            damage = target.TakeDamage(damage);
            return new SkillResultEntry(targetUnit, damage, target.HasZeroHealth, SkillResultType.Hit);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static int CeilToInt(float value)
        {
            return (int)System.Math.Ceiling(value);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}