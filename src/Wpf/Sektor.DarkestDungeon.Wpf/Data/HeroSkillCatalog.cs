using System;
using System.Collections.Generic;
using System.Windows.Media;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>
    /// Builds the skill list of a hero class from the parsed content (<c>info:</c> skills plus the
    /// <c>art:</c> icon/animation ids) and the bundled ability icons.
    /// </summary>
    public sealed class HeroSkillCatalog : IHeroSkillCatalog
    {
        private readonly Dictionary<string, HeroClass> classesById;
        private readonly IHeroIconCatalog iconCatalog;

        /// <summary>Initializes a new instance of the <see cref="HeroSkillCatalog"/> class.</summary>
        /// <param name="classes">The parsed hero classes.</param>
        /// <param name="iconCatalog">The catalog of bundled ability icons.</param>
        public HeroSkillCatalog(IEnumerable<HeroClass> classes, IHeroIconCatalog iconCatalog)
        {
            if (classes == null)
                throw new ArgumentNullException(nameof(classes));

            this.iconCatalog = iconCatalog ?? throw new ArgumentNullException(nameof(iconCatalog));
            classesById = new Dictionary<string, HeroClass>(StringComparer.Ordinal);
            foreach (var heroClass in classes)
            {
                if (heroClass != null && !string.IsNullOrEmpty(heroClass.StringId))
                    classesById[heroClass.StringId] = heroClass;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<HeroSkillInfo> GetSkills(string classId)
        {
            HeroClass? heroClass;
            if (string.IsNullOrWhiteSpace(classId) || !classesById.TryGetValue(classId, out heroClass))
                return Array.Empty<HeroSkillInfo>();

            var skills = new List<HeroSkillInfo>(heroClass.CombatSkills.Count);
            foreach (var skill in heroClass.CombatSkills)
            {
                var art = FindArt(heroClass, skill.Id);
                skills.Add(new HeroSkillInfo(skill, art, ResolveIcon(heroClass, art)));
            }
            return skills;
        }

        /// <inheritdoc/>
        public ImageSource? GetSkillIcon(string classId, string skillId)
        {
            HeroClass? heroClass;
            if (string.IsNullOrWhiteSpace(classId)
                || string.IsNullOrWhiteSpace(skillId)
                || !classesById.TryGetValue(classId, out heroClass))
            {
                return null;
            }

            return ResolveIcon(heroClass, FindArt(heroClass, skillId));
        }

        private ImageSource? ResolveIcon(HeroClass heroClass, SkillArtInfo? art)
        {
            return art?.IconId == null ? null : iconCatalog.GetAbilityIcon(heroClass.StringId, art.IconId);
        }

        private static SkillArtInfo? FindArt(HeroClass heroClass, string skillId)
        {
            foreach (var art in heroClass.SkillArtInfo)
            {
                if (string.Equals(art.SkillId, skillId, StringComparison.Ordinal))
                    return art;
            }

            return null;
        }
    }
}
