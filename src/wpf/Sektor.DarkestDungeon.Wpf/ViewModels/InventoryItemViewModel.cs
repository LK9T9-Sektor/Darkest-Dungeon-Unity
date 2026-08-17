using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single inventory slot (item icon placeholder + stack count).</summary>
    public partial class InventoryItemViewModel : ObservableObject
    {
        /// <summary>Gets the item name placeholder (empty for free slots).</summary>
        public string Name { get; }

        /// <summary>Gets the stack amount.</summary>
        public int Quantity { get; }

        /// <summary>Gets a value indicating whether the slot is empty.</summary>
        public bool IsEmpty { get { return string.IsNullOrEmpty(Name); } }

        /// <summary>Gets the display text for the slot.</summary>
        public string DisplayText { get { return IsEmpty ? string.Empty : Name + " x" + Quantity; } }

        /// <summary>Initializes a new instance of the <see cref="InventoryItemViewModel"/> class.</summary>
        public InventoryItemViewModel(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}
