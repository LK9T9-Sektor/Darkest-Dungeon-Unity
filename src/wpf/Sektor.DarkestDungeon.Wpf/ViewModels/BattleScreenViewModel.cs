using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Root view model of the battle screen mockup.</summary>
    public partial class BattleScreenViewModel : ObservableObject
    {
        /// <summary>Gets the torch meter view model.</summary>
        public TorchViewModel Torch { get; } = new TorchViewModel();

        /// <summary>Gets the quest log view model.</summary>
        public QuestLogViewModel QuestLog { get; } = new QuestLogViewModel();

        /// <summary>Gets the bottom raid HUD view model.</summary>
        public RaidHudViewModel RaidHud { get; } = new RaidHudViewModel();

        /// <summary>Gets the events overlay view model (round, announcement, popup).</summary>
        public EventsLayerViewModel Events { get; } = new EventsLayerViewModel();

        /// <summary>Gets the player party units on stage.</summary>
        public ObservableCollection<UnitViewModel> Heroes { get; } = new ObservableCollection<UnitViewModel>();

        /// <summary>Gets the enemy units on stage.</summary>
        public ObservableCollection<UnitViewModel> Monsters { get; } = new ObservableCollection<UnitViewModel>();

        /// <summary>Gets or sets the currently hovered/selected unit shown in the monster tooltip.</summary>
        [ObservableProperty]
        private UnitViewModel? _tooltipTarget;

        /// <summary>Initializes the mockup with placeholder stage data.</summary>
        public BattleScreenViewModel()
        {
            Heroes.Add(new UnitViewModel("Reynauld", "Crusader", 100, 100, 20));
            Heroes.Add(new UnitViewModel("Dismas", "Highwayman", 88, 92, 35));
            Heroes.Add(new UnitViewModel("Paracelsus", "Plague Doctor", 72, 85, 10));
            Heroes.Add(new UnitViewModel("Junia", "Vestal", 64, 78, 15));

            Monsters.Add(new UnitViewModel("Cultist Acolyte", "Cultist", 40, 40, 40, true));
            Monsters.Add(new UnitViewModel("Bone Soldier", "Bone", 55, 55, 25, true));
            Monsters.Add(new UnitViewModel("Swine Wretch", "Swine", 48, 48, 5, true));
        }
    }
}
