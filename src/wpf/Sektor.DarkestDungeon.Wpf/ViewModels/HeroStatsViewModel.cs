using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Stat sheet of the selected hero.</summary>
    public partial class HeroStatsViewModel : ObservableObject
    {
        /// <summary>Gets the current hit points.</summary>
        public string HitPoints { get; } = "78 / 78";

        /// <summary>Gets the current stress value.</summary>
        public string Stress { get; } = "0 / 100";

        /// <summary>Gets the speed value.</summary>
        public string Speed { get; } = "4";

        /// <summary>Gets the damage range.</summary>
        public string Damage { get; } = "8 - 15";

        /// <summary>Gets the accuracy modifier.</summary>
        public string Accuracy { get; } = "+85";

        /// <summary>Gets the critical chance.</summary>
        public string Crit { get; } = "7%";

        /// <summary>Gets the dodge value.</summary>
        public string Dodge { get; } = "15";

        /// <summary>Gets the protection value.</summary>
        public string Protection { get; } = "10%";

        /// <summary>Gets the weapon level label.</summary>
        public string WeaponLevel { get; } = "Lv. 1";

        /// <summary>Gets the armor level label.</summary>
        public string ArmorLevel { get; } = "Lv. 1";
    }
}
