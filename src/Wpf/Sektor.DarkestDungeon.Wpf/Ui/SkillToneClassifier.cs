using System.Windows.Media;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Classifies a combat skill into a visual tone and maps it to the shared arrow colors.</summary>
    public static class SkillToneClassifier
    {
        /// <summary>Gets the arrow color for attack skills (red).</summary>
        public static readonly Brush AttackBrush = Frozen(Color.FromRgb(0xC0, 0x39, 0x2B));

        /// <summary>Gets the arrow color for heal skills (green).</summary>
        public static readonly Brush HealBrush = Frozen(Color.FromRgb(0x2E, 0x8A, 0x4A));

        /// <summary>Gets the arrow color for buff skills (blue).</summary>
        public static readonly Brush BuffBrush = Frozen(Color.FromRgb(0x3A, 0x6A, 0xB0));

        /// <summary>Classifies a skill by what it does (damage, heal or buff).</summary>
        /// <param name="skill">The combat skill, or null.</param>
        /// <returns>The tone.</returns>
        public static SkillTone Classify(CombatSkill? skill)
        {
            if (skill == null)
                return SkillTone.Attack;

            if (skill.Heal != null)
                return SkillTone.Heal;

            if (skill.Category == SkillCategory.Damage)
                return SkillTone.Attack;

            return SkillTone.Buff;
        }

        /// <summary>Gets the arrow brush for a tone.</summary>
        /// <param name="tone">The tone.</param>
        /// <returns>The brush.</returns>
        public static Brush ArrowBrush(SkillTone tone)
        {
            switch (tone)
            {
                case SkillTone.Heal:
                    return HealBrush;
                case SkillTone.Buff:
                    return BuffBrush;
                default:
                    return AttackBrush;
            }
        }

        private static Brush Frozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}