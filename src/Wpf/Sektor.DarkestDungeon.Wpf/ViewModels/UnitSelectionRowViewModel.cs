using System;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single selectable unit row in the lobby selection table.</summary>
    public sealed class UnitSelectionRowViewModel
    {
        private const int SingleSlotSize = 1;

        private readonly Action<HeroSlotViewModel> assign;

        /// <summary>Initializes a new instance of the <see cref="UnitSelectionRowViewModel"/> class.</summary>
        /// <param name="icon">The optional row icon (hero portrait); null when no icon exists.</param>
        /// <param name="displayName">The class or unit name shown in the table.</param>
        /// <param name="size">The number of formation slots the unit occupies.</param>
        /// <param name="assign">The action that places this unit into a slot.</param>
        public UnitSelectionRowViewModel(
            ImageSource? icon,
            string displayName,
            int size,
            Action<HeroSlotViewModel> assign)
        {
            Icon = icon;
            DisplayName = displayName;
            Size = size;
            this.assign = assign;
        }

        /// <summary>Gets the optional icon; only heroes have icons for now.</summary>
        public ImageSource? Icon { get; }

        /// <summary>Gets the class or unit name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets the number of formation slots the unit occupies.</summary>
        public int Size { get; }

        /// <summary>Gets a value indicating whether the row can be selected (one-slot units only).</summary>
        public bool IsSelectable { get { return Size == SingleSlotSize; } }

        /// <summary>Places this unit into the supplied slot.</summary>
        /// <param name="slot">The target slot.</param>
        public void AssignTo(HeroSlotViewModel slot)
        {
            assign(slot);
        }
    }
}
