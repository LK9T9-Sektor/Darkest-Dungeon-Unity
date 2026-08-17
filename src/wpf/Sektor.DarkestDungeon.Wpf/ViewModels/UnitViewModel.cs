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

        /// <summary>Gets or sets a value indicating whether the unit is an enemy.</summary>
        public bool IsEnemy { get; }

        /// <summary>Gets or sets a value indicating whether the unit is currently selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

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
