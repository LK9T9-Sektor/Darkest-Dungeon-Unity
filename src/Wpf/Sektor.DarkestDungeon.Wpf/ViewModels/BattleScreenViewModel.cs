using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        /// <summary>Gets the turn order panel placeholder.</summary>
        public TurnOrderViewModel TurnOrder { get; } = new TurnOrderViewModel();

        /// <summary>Gets the four fixed player party slots (some may be empty).</summary>
        public ObservableCollection<UnitSlotViewModel> HeroSlots { get; } = new ObservableCollection<UnitSlotViewModel>();

        /// <summary>Gets the four fixed enemy slots (some may be empty).</summary>
        public ObservableCollection<UnitSlotViewModel> MonsterSlots { get; } = new ObservableCollection<UnitSlotViewModel>();

        /// <summary>Gets or sets the currently hovered/selected unit shown in the monster tooltip.</summary>
        [ObservableProperty]
        private UnitViewModel? _tooltipTarget;

        /// <summary>Gets the stat sheet shown when a unit is right-clicked.</summary>
        public HeroStatsViewModel StatsTarget { get; } = new HeroStatsViewModel();

        /// <summary>Gets or sets a value indicating whether the stats sheet overlay is visible.</summary>
        [ObservableProperty]
        private bool _isStatsVisible;

        /// <summary>Gets the command that opens the stats sheet for the given unit.</summary>
        public IRelayCommand<UnitViewModel?> OpenStatsCommand { get; }

        /// <summary>Gets the command that closes the stats sheet.</summary>
        public IRelayCommand CloseStatsCommand { get; }

        /// <summary>Gets the command raised when the mouse hovers a stage unit.</summary>
        public IRelayCommand<UnitViewModel?> HoverCommand { get; }

        /// <summary>Gets the command raised when the mouse leaves a stage unit.</summary>
        public IRelayCommand UnhoverCommand { get; }

        /// <summary>Initializes the mockup with placeholder stage data.</summary>
        public BattleScreenViewModel()
        {
            OpenStatsCommand = new RelayCommand<UnitViewModel?>(OpenStats);
            CloseStatsCommand = new RelayCommand(() => IsStatsVisible = false);
            HoverCommand = new RelayCommand<UnitViewModel?>(Hover);
            UnhoverCommand = new RelayCommand(Unhover);

            var heroes = new[]
            {
                new UnitViewModel("Reynauld", "Crusader", 100, 100, 20),
                new UnitViewModel("Dismas", "Highwayman", 88, 92, 35),
                new UnitViewModel("Paracelsus", "Plague Doctor", 72, 85, 10),
                new UnitViewModel("Junia", "Vestal", 64, 78, 15),
            };
            foreach (var hero in heroes)
                HeroSlots.Add(new UnitSlotViewModel(hero));
            heroes[0].Tray[0].IsActive = true;

            var monsters = new[]
            {
                new UnitViewModel("Cultist Acolyte", "Cultist", 40, 40, 40, true),
                new UnitViewModel("Bone Soldier", "Bone", 55, 55, 25, true),
                new UnitViewModel("Swine Wretch", "Swine", 48, 48, 5, true),
            };
            foreach (var monster in monsters)
                MonsterSlots.Add(new UnitSlotViewModel(monster));
            MonsterSlots.Add(new UnitSlotViewModel(null));
            monsters[0].Tray[1].IsActive = true;
            monsters[0].Tray[2].IsActive = true;
        }

        private void OpenStats(UnitViewModel? unit)
        {
            if (unit == null)
                return;

            StatsTarget.Apply(unit);
            IsStatsVisible = true;
        }

        private void Hover(UnitViewModel? unit)
        {
            if (unit == null)
                return;

            unit.IsSelected = true;
            TooltipTarget = unit;
        }

        private void Unhover()
        {
            if (TooltipTarget != null)
                TooltipTarget.IsSelected = false;
            TooltipTarget = null;
        }
    }
}
