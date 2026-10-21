using System.Collections.Generic;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Provides the combat skills of a hero class together with their bundled art.</summary>
    public interface IHeroSkillCatalog
    {
        /// <summary>Gets the base-rank combat skills of a hero, in content order.</summary>
        /// <param name="classId">The snake_case hero class id ("crusader").</param>
        /// <returns>The skills with icons and animation ids; empty when the class is unknown.</returns>
        IReadOnlyList<HeroSkillInfo> GetSkills(string classId);

        /// <summary>Gets the bundled icon of one skill of a hero class.</summary>
        /// <param name="classId">The snake_case hero class id ("crusader").</param>
        /// <param name="skillId">The combat skill id ("smite").</param>
        /// <returns>The frozen icon, or null when the class/skill has no bundled icon.</returns>
        ImageSource? GetSkillIcon(string classId, string skillId);
    }
}
