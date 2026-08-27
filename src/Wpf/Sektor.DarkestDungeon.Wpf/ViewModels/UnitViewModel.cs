using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single unit (hero or monster) standing on the battle stage.</summary>
    public partial class UnitViewModel : ObservableObject
    {
        /// <summary>Gets the display name of the unit.</summary>
        public string Name { get; }

        /// <summary>Gets the class/type label of the unit.</summary>
        public string ClassName { get; }

        /// <summary>Gets or sets the current hit points.</summary>
        [ObservableProperty]
        private int _hpCurrent;

        /// <summary>Gets or sets the maximum hit points.</summary>
        [ObservableProperty]
        private int _hpMax;

        /// <summary>Gets or sets the stress value (0-100).</summary>
        [ObservableProperty]
        private int _stress;

        /// <summary>Gets or sets the speed value (placeholder).</summary>
        [ObservableProperty]
        private int _speed = 5;

        /// <summary>Gets or sets the protection percentage (placeholder).</summary>
        [ObservableProperty]
        private int _protection;

        /// <summary>Gets or sets the stun resistance percentage (placeholder).</summary>
        [ObservableProperty]
        private int _resistStun = 20;

        /// <summary>Gets or sets the blight resistance percentage (placeholder).</summary>
        [ObservableProperty]
        private int _resistBlight = 10;

        /// <summary>Gets or sets the bleed resistance percentage (placeholder).</summary>
        [ObservableProperty]
        private int _resistBleed = 10;

        /// <summary>Gets or sets the debuff resistance percentage (placeholder).</summary>
        [ObservableProperty]
        private int _resistDebuff = 15;

        /// <summary>Gets or sets the move resistance percentage (placeholder).</summary>
        [ObservableProperty]
        private int _resistMove = 25;

        /// <summary>Gets or sets a value indicating whether the unit is an enemy.</summary>
        public bool IsEnemy { get; }

        /// <summary>Gets or sets a value indicating whether the unit is currently selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Gets the buff/debuff/status tray icons floating above the unit.</summary>
        public ObservableCollection<TraySlotViewModel> Tray { get; } = new ObservableCollection<TraySlotViewModel>();

        /// <summary>Gets the hit point ratio (0-1) for the health bar.</summary>
        public double HpRatio
        {
            get { return HpMax <= 0 ? 0 : (double)HpCurrent / HpMax; }
        }

        /// <summary>Gets the fixed stress pip markers shown under the health bar.</summary>
        public System.Collections.Generic.IEnumerable<int> StressPips { get; } = System.Linq.Enumerable.Repeat(0, 10);

        /// <summary>Initializes a new instance of the <see cref="UnitViewModel"/> class.</summary>
        public UnitViewModel(string name, string className, int hpCurrent, int hpMax, int stress, bool isEnemy = false)
        {
            Name = name;
            ClassName = className;
            _hpCurrent = hpCurrent;
            _hpMax = hpMax;
            _stress = stress;
            IsEnemy = isEnemy;

            Tray.Add(new TraySlotViewModel("B", "Buff", "Combat buffs"));
            Tray.Add(new TraySlotViewModel("D", "Debuff", "Combat debuffs"));
            Tray.Add(new TraySlotViewModel("Bd", "Dot", "Bleeding"));
            Tray.Add(new TraySlotViewModel("Po", "Dot", "Poisoned"));
            Tray.Add(new TraySlotViewModel("T", "Debuff", "Marked"));
            Tray.Add(new TraySlotViewModel("St", "Debuff", "Stunned"));
        }

        partial void OnHpCurrentChanged(int value)
        {
            OnPropertyChanged(nameof(HpRatio));
        }

        partial void OnHpMaxChanged(int value)
        {
            OnPropertyChanged(nameof(HpRatio));
        }
    }
}
