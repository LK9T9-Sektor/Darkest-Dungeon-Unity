using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A fixed battle slot that either holds a unit or stays empty but visible.</summary>
    public partial class UnitSlotViewModel : ObservableObject
    {
        /// <summary>Gets the unit standing in the slot (null for an empty slot).</summary>
        public UnitViewModel? Unit { get; }

        /// <summary>Gets a value indicating whether the slot is occupied.</summary>
        public bool HasUnit { get { return Unit != null; } }

        /// <summary>Initializes a new instance of the <see cref="UnitSlotViewModel"/> class.</summary>
        public UnitSlotViewModel(UnitViewModel? unit)
        {
            Unit = unit;
        }
    }
}
