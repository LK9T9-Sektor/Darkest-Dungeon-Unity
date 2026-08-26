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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Single-target skill result entry.</summary>
    public class SkillResultEntry
    {
        /// <summary>Gets or sets the damage amount.</summary>
        public int Amount { get; set; }

        /// <summary>Gets or sets a value indicating whether the target was zeroed (killed).</summary>
        public bool IsZeroed { get; set; }

        /// <summary>Gets or sets a value indicating whether the target was hit.</summary>
        public bool IsTargetHit { get; set; }

        /// <summary>Gets or sets a value indicating whether the hit was harmful.</summary>
        public bool IsHarmful { get; set; }

        /// <summary>Gets or sets a value indicating whether this crit relieves stress.</summary>
        public bool CanCritReleaf { get; set; }

        /// <summary>Gets or sets a value indicating whether this kill relieves stress.</summary>
        public bool CanKillReleaf { get; set; }

        /// <summary>Gets or sets the result type.</summary>
        public SkillResultType Type { get; set; }

        /// <summary>Gets or sets the target unit.</summary>
        public ICombatUnit Target { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillResultEntry"/> class.</summary>
        /// <param name="target">The target unit.</param>
        /// <param name="result">The result type.</param>
        public SkillResultEntry(ICombatUnit target, SkillResultType result)
        {
            Type = result;
            Target = target;
            IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
            IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        }

        /// <summary>Initializes a new instance of the <see cref="SkillResultEntry"/> class.</summary>
        /// <param name="target">The target unit.</param>
        /// <param name="skillDamage">The damage amount.</param>
        /// <param name="result">The result type.</param>
        public SkillResultEntry(ICombatUnit target, int skillDamage, SkillResultType result)
        {
            Amount = skillDamage;
            Type = result;
            Target = target;
            IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
            IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        }

        /// <summary>Initializes a new instance of the <see cref="SkillResultEntry"/> class.</summary>
        /// <param name="target">The target unit.</param>
        /// <param name="skillDamage">The damage amount.</param>
        /// <param name="isTargetZeroed">Whether the target was zeroed.</param>
        /// <param name="result">The result type.</param>
        public SkillResultEntry(ICombatUnit target, int skillDamage, bool isTargetZeroed, SkillResultType result)
        {
            Amount = skillDamage;
            Type = result;
            Target = target;
            IsZeroed = isTargetZeroed;
            IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
            IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        }
    }
}
