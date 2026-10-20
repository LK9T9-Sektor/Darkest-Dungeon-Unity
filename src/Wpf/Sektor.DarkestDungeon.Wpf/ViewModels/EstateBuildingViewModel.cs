using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One building card in the estate replay.</summary>
    public sealed class EstateBuildingViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="EstateBuildingViewModel"/> class.</summary>
        /// <param name="building">The building state.</param>
        public EstateBuildingViewModel(EstateBuildingState building)
        {
            Id = building.Id;
            Name = building.Name;
            ProgressPercent = building.UpgradeRatio * 100.0;
            ProgressText = $"{building.UpgradeRatio:P0}";
        }

        /// <summary>Gets the building id.</summary>
        public string Id { get; }

        /// <summary>Gets the building name.</summary>
        public string Name { get; }

        /// <summary>Gets the upgrade progress in percent (0..100).</summary>
        public double ProgressPercent { get; }

        /// <summary>Gets the upgrade progress label.</summary>
        public string ProgressText { get; }
    }
}
