using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Provides the bundled hero icons (roster portraits of every outfit) to screens.</summary>
    public interface IHeroIconCatalog
    {
        /// <summary>Gets the roster portrait of one hero outfit.</summary>
        /// <param name="classId">The snake_case hero class id ("man_at_arms").</param>
        /// <param name="outfit">The outfit letter ("A".."D").</param>
        /// <returns>The frozen portrait image, or null when the hero/outfit has no bundled portrait.</returns>
        ImageSource? GetPortrait(string classId, string outfit);

        /// <summary>Gets the combat skill icon of one hero.</summary>
        /// <param name="classId">The snake_case hero class id ("man_at_arms").</param>
        /// <param name="iconId">The skill art icon id ("one".."seven").</param>
        /// <returns>The frozen icon image, or null when the hero has no bundled icon with this id.</returns>
        ImageSource? GetAbilityIcon(string classId, string iconId);
    }
}
