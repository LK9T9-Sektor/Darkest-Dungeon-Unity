using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Bottom raid HUD: selected hero, stats, skills, inventory/map toggle.</summary>
    public partial class RaidHudViewModel : ObservableObject
    {
        /// <summary>Gets the hero banner + stats view model.</summary>
        public HeroViewModel Hero { get; } = new HeroViewModel();

        /// <summary>Gets the party inventory view model.</summary>
        public InventoryViewModel Inventory { get; } = new InventoryViewModel();

        /// <summary>Gets the dungeon map view model.</summary>
        public MapViewModel Map { get; } = new MapViewModel();

        /// <summary>Gets or sets a value indicating whether the right panel shows the inventory (true) or the map (false).</summary>
        [ObservableProperty]
        private bool _isInventoryShown = true;

        /// <summary>Gets the command that switches the right panel to the inventory.</summary>
        public IRelayCommand ShowInventoryCommand { get; }

        /// <summary>Gets the command that switches the right panel to the map.</summary>
        public IRelayCommand ShowMapCommand { get; }

        /// <summary>Gets the command that toggles the right panel between inventory and map.</summary>
        public IRelayCommand ToggleRightPanelCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="RaidHudViewModel"/> class.</summary>
        public RaidHudViewModel()
        {
            ShowInventoryCommand = new RelayCommand(() => IsInventoryShown = true);
            ShowMapCommand = new RelayCommand(() => IsInventoryShown = false);
            ToggleRightPanelCommand = new RelayCommand(ToggleRightPanel);
        }

        private void ToggleRightPanel()
        {
            IsInventoryShown = !IsInventoryShown;
        }
    }
}
