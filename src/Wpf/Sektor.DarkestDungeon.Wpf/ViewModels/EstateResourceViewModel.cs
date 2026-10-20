using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One estate resource line (gold or an heirloom).</summary>
    public sealed class EstateResourceViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="EstateResourceViewModel"/> class.</summary>
        /// <param name="resource">The resource state.</param>
        public EstateResourceViewModel(EstateResourceAmount resource)
        {
            Id = resource.Id;
            Amount = resource.Amount;
        }

        /// <summary>Gets the resource id.</summary>
        public string Id { get; }

        /// <summary>Gets the resource amount.</summary>
        public int Amount { get; }

        /// <summary>Gets the display line.</summary>
        public string Label => $"{Id}: {Amount}";
    }
}
