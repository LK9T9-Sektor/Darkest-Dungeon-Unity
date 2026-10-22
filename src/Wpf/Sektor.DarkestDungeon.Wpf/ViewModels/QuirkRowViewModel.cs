using System.Windows.Media;

using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A quirk row in the hero editor: display name plus its optional lock marker.</summary>
    public sealed class QuirkRowViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="QuirkRowViewModel"/> class.</summary>
        /// <param name="quirk">The quirk definition.</param>
        /// <param name="isLocked">Whether the quirk is locked for this hero.</param>
        public QuirkRowViewModel(Quirk quirk, bool isLocked)
        {
            Id = quirk.Id;
            DisplayName = DisplayNames.Title(quirk.Id);
            IsPositive = quirk.IsPositive;
            IsLocked = isLocked;
            LockIcon = isLocked ? QuirkIconCatalog.LockIcon : null;
        }

        private QuirkRowViewModel(bool isPositive)
        {
            Id = string.Empty;
            DisplayName = string.Empty;
            IsPositive = isPositive;
            IsLocked = false;
            LockIcon = null;
        }

        /// <summary>Creates an empty row used to reserve the configured default height.</summary>
        /// <param name="isPositive">Whether the empty row belongs to the positive column.</param>
        /// <returns>An empty quirk row.</returns>
        public static QuirkRowViewModel Empty(bool isPositive)
        {
            return new QuirkRowViewModel(isPositive);
        }

        /// <summary>Gets the raw quirk id.</summary>
        public string Id { get; }

        /// <summary>Gets the readable quirk name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets a value indicating whether the quirk is positive.</summary>
        public bool IsPositive { get; }

        /// <summary>Gets a value indicating whether the quirk is locked.</summary>
        public bool IsLocked { get; }

        /// <summary>Gets the lock icon for a locked quirk, or null for an unlocked quirk.</summary>
        public ImageSource? LockIcon { get; }
    }
}
