using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One line of the capture timeline.</summary>
    public partial class CaptureTimelineEntryViewModel : ObservableObject
    {
        /// <summary>Initializes a new instance of the <see cref="CaptureTimelineEntryViewModel"/> class.</summary>
        /// <param name="sequence">The event sequence number.</param>
        /// <param name="description">The human-readable event description.</param>
        public CaptureTimelineEntryViewModel(long sequence, string description)
        {
            Sequence = sequence;
            Description = description;
        }

        /// <summary>Gets the event sequence number.</summary>
        public long Sequence { get; }

        /// <summary>Gets the event description.</summary>
        public string Description { get; }

        /// <summary>Gets the label shown in the timeline list.</summary>
        public string Label => $"{Sequence}. {Description}";

        /// <summary>Gets or sets a value indicating whether this line is the current replay step.</summary>
        [ObservableProperty]
        private bool _isCurrent;

        /// <summary>Gets or sets a value indicating whether this line was already replayed.</summary>
        [ObservableProperty]
        private bool _isPast;
    }
}
