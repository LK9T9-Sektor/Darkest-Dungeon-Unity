using System;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Event data for a request to open the loadout editor for one hero slot.</summary>
    public sealed class HeroSlotEditRequestedEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="HeroSlotEditRequestedEventArgs"/> class.</summary>
        /// <param name="slot">The slot to edit.</param>
        public HeroSlotEditRequestedEventArgs(HeroSlotViewModel slot)
        {
            Slot = slot ?? throw new ArgumentNullException(nameof(slot));
        }

        /// <summary>Gets the slot to edit.</summary>
        public HeroSlotViewModel Slot { get; }
    }
}
