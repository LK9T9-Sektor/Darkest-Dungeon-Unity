namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One disease slot in the hero editor. An empty slot is rendered as a fixed
    /// placeholder so the diseases row always keeps three positions.</summary>
    public sealed class DiseaseRowViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="DiseaseRowViewModel"/> class.</summary>
        /// <param name="displayName">The disease name, or empty for an unused slot.</param>
        public DiseaseRowViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        /// <summary>Gets the disease name, or an empty string for an unused slot.</summary>
        public string DisplayName { get; }

        /// <summary>Gets a value indicating whether this is an unused slot.</summary>
        public bool IsEmpty { get { return string.IsNullOrEmpty(DisplayName); } }
    }
}
