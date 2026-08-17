using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single buff/debuff/status tray icon above a unit.</summary>
    public partial class TraySlotViewModel : ObservableObject
    {
        /// <summary>Gets the short label used as the placeholder icon.</summary>
        public string Label { get; }

        /// <summary>Gets the icon tone: "Buff", "Debuff" or "Dot".</summary>
        public string Tone { get; }

        /// <summary>Gets the tooltip text describing the status.</summary>
        public string Tooltip { get; }

        /// <summary>Gets or sets a value indicating whether the status is applied.</summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>Initializes a new instance of the <see cref="TraySlotViewModel"/> class.</summary>
        public TraySlotViewModel(string label, string tone, string tooltip)
        {
            Label = label;
            Tone = tone;
            Tooltip = tooltip;
        }
    }
}
