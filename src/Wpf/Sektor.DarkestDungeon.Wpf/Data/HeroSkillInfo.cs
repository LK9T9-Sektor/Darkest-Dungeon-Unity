using System.Windows.Media;

using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>A hero combat skill together with its bundled icon and art animation ids.</summary>
    public sealed class HeroSkillInfo
    {
        /// <summary>Initializes a new instance of the <see cref="HeroSkillInfo"/> class.</summary>
        /// <param name="skill">The combat skill.</param>
        /// <param name="art">The skill art entry (icon and animation ids), or null when the content has none.</param>
        /// <param name="icon">The bundled ability icon, or null when no icon is delivered.</param>
        public HeroSkillInfo(CombatSkill skill, SkillArtInfo? art, ImageSource? icon)
        {
            Skill = skill;
            Art = art;
            Icon = icon;
        }

        /// <summary>Gets the combat skill.</summary>
        public CombatSkill Skill { get; }

        /// <summary>Gets the skill art entry (icon and animation ids), or null when the content has none.</summary>
        public SkillArtInfo? Art { get; }

        /// <summary>Gets the bundled ability icon, or null when no icon is delivered.</summary>
        public ImageSource? Icon { get; }

        /// <summary>Gets the effect animation id (<c>.fx</c>) played on hit, or null when the skill has none.</summary>
        public string? EffectAnimationId => Art?.ArtId;

        /// <summary>Gets the base attack pose animation id (<c>.anim</c>), or null when the content has none.</summary>
        public string? BaseAnimationId => Art?.AnimationId;
    }
}
