using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Bottom raid HUD: acting hero info, tooltip, log/inventory/map toggle.</summary>
    public partial class RaidHudViewModel : ObservableObject
    {
        /// <summary>Gets the hero banner + stats view model.</summary>
        public HeroViewModel Hero { get; } = new HeroViewModel();

        /// <summary>Gets the party inventory view model.</summary>
        public InventoryViewModel Inventory { get; } = new InventoryViewModel();

        /// <summary>Gets the dungeon map view model.</summary>
        public MapViewModel Map { get; } = new MapViewModel();

        /// <summary>Gets or sets a value indicating whether the right panel shows the battle log.</summary>
        [ObservableProperty]
        private bool _isLogShown = true;

        /// <summary>Gets or sets a value indicating whether the right panel shows the inventory.</summary>
        [ObservableProperty]
        private bool _isInventoryShown;

        /// <summary>Gets or sets a value indicating whether the right panel shows the map.</summary>
        [ObservableProperty]
        private bool _isMapShown;

        /// <summary>Gets the command that switches the right panel to the battle log.</summary>
        public IRelayCommand ShowLogCommand { get; }

        /// <summary>Gets the command that switches the right panel to the inventory.</summary>
        public IRelayCommand ShowInventoryCommand { get; }

        /// <summary>Gets the command that switches the right panel to the map.</summary>
        public IRelayCommand ShowMapCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="RaidHudViewModel"/> class.</summary>
        public RaidHudViewModel()
        {
            ShowLogCommand = new RelayCommand(() => SetPanel(true, false, false));
            ShowInventoryCommand = new RelayCommand(() => SetPanel(false, true, false));
            ShowMapCommand = new RelayCommand(() => SetPanel(false, false, true));
        }

        /// <summary>Fills the hero panel with the acting unit's live data.</summary>
        /// <param name="name">The unit name.</param>
        /// <param name="className">The class label.</param>
        /// <param name="combatSkills">The unit's combat skills.</param>
        /// <param name="hpCurrent">The current hit points.</param>
        /// <param name="hpMax">The maximum hit points.</param>
        /// <param name="stress">The stress value.</param>
        /// <param name="speed">The speed value.</param>
        /// <param name="minDamage">The minimum damage.</param>
        /// <param name="maxDamage">The maximum damage.</param>
        /// <param name="accuracy">The accuracy.</param>
        /// <param name="crit">The critical chance percentage.</param>
        /// <param name="dodge">The dodge value.</param>
        /// <param name="protection">The protection percentage.</param>
        public void ApplyActor(
            string name,
            string className,
            IEnumerable<CombatSkill> combatSkills,
            int hpCurrent,
            int hpMax,
            int stress,
            int speed,
            int minDamage,
            int maxDamage,
            int accuracy,
            int crit,
            int dodge,
            int protection)
        {
            Hero.Apply(name, className, combatSkills, hpCurrent, hpMax, stress, speed, minDamage, maxDamage, accuracy, crit, dodge, protection);
        }

        private void SetPanel(bool log, bool inventory, bool map)
        {
            IsLogShown = log;
            IsInventoryShown = inventory;
            IsMapShown = map;
        }
    }
}