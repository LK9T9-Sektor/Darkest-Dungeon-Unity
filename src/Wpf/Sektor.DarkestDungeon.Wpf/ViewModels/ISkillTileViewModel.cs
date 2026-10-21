using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Common contract for a skill square: icon, label, selection and availability.</summary>
    public interface ISkillTileViewModel
    {
        /// <summary>Gets the skill icon, or null when no icon is bundled.</summary>
        ImageSource? Icon { get; }

        /// <summary>Gets the uppercase label shown under the icon.</summary>
        string DisplayNameUpper { get; }

        /// <summary>Gets a value indicating whether the tile can currently be clicked to toggle the skill.</summary>
        bool IsUsable { get; }

        /// <summary>Gets a value indicating whether the skill is selected/active.</summary>
        bool IsSelected { get; }
    }
}
