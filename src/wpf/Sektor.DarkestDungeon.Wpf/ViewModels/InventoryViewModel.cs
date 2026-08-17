using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Party inventory grid (8 columns x 2 rows) with placeholder items.</summary>
    public partial class InventoryViewModel : ObservableObject
    {
        /// <summary>Gets the item slots of the inventory grid.</summary>
        public ObservableCollection<InventoryItemViewModel> Items { get; } = new ObservableCollection<InventoryItemViewModel>();

        /// <summary>Initializes a new instance of the <see cref="InventoryViewModel"/> class.</summary>
        public InventoryViewModel()
        {
            Items.Add(new InventoryItemViewModel("Ration", 4));
            Items.Add(new InventoryItemViewModel("Torch", 6));
            Items.Add(new InventoryItemViewModel("Shovel", 2));
            Items.Add(new InventoryItemViewModel("Bandage", 3));
            Items.Add(new InventoryItemViewModel("Key", 1));
            Items.Add(new InventoryItemViewModel("Antivenom", 2));
            Items.Add(new InventoryItemViewModel("Holy Water", 1));
            Items.Add(new InventoryItemViewModel("Herbs", 3));
            for (int i = Items.Count; i < 16; i++)
                Items.Add(new InventoryItemViewModel(string.Empty, 0));
        }
    }
}
