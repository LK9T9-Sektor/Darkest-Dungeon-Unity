using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A unit card in the live duel battle view.</summary>
    public partial class DuelUnitViewModel : ObservableObject
    {
        /// <summary>Gets the combat id.</summary>
        public int CombatId { get; }

        /// <summary>Gets the display name.</summary>
        public string Name { get; }

        /// <summary>Gets the class label.</summary>
        public string ClassName { get; }

        /// <summary>Gets or sets the current hit points.</summary>
        [ObservableProperty]
        private int _hpCurrent;

        /// <summary>Gets or sets the maximum hit points.</summary>
        [ObservableProperty]
        private int _hpMax;

        /// <summary>Gets or sets the stress value.</summary>
        [ObservableProperty]
        private int _stress;

        /// <summary>Gets or sets the speed value.</summary>
        [ObservableProperty]
        private int _speed;

        /// <summary>Gets or sets the damage range text ("min - max").</summary>
        [ObservableProperty]
        private string _damage = string.Empty;

        /// <summary>Gets or sets the accuracy value.</summary>
        [ObservableProperty]
        private int _accuracy;

        /// <summary>Gets or sets the critical chance percentage.</summary>
        [ObservableProperty]
        private int _crit;

        /// <summary>Gets or sets the dodge value.</summary>
        [ObservableProperty]
        private int _dodge;

        /// <summary>Gets or sets the protection percentage.</summary>
        [ObservableProperty]
        private int _protection;

        /// <summary>Gets or sets a value indicating whether this unit is the current acting unit.</summary>
        [ObservableProperty]
        private bool _isCurrent;

        /// <summary>Gets or sets a value indicating whether this unit is highlighted as a target.</summary>
        [ObservableProperty]
        private bool _isTarget;

        /// <summary>Gets or sets a value indicating whether this unit is hovered.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Gets or sets a value indicating whether the unit belongs to the enemy side.</summary>
        [ObservableProperty]
        private bool _isEnemy;

        /// <summary>Gets the hit point ratio (0-1) for the health bar.</summary>
        public double HpRatio { get { return HpMax <= 0 ? 0 : (double)HpCurrent / HpMax; } }

        /// <summary>Gets the hit points text.</summary>
        public string HpText { get { return HpCurrent + " / " + HpMax; } }

        /// <summary>Initializes a new instance of the <see cref="DuelUnitViewModel"/> class.</summary>
        /// <param name="combatId">The combat id.</param>
        /// <param name="name">The name.</param>
        /// <param name="className">The class label.</param>
        public DuelUnitViewModel(int combatId, string name, string className)
        {
            CombatId = combatId;
            Name = name;
            ClassName = className;
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