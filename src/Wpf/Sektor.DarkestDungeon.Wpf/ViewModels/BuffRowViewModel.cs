namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single buff or debuff row shown in the card's status table popup.</summary>
    public class BuffRowViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="BuffRowViewModel"/> class.</summary>
        /// <param name="name">The display name.</param>
        /// <param name="durationText">The duration or charge text.</param>
        /// <param name="description">The effect description.</param>
        /// <param name="tone">The tone ("Buff" or "Debuff").</param>
        public BuffRowViewModel(string name, string durationText, string description, string tone)
        {
            Name = name;
            DurationText = durationText;
            Description = description;
            Tone = tone;
        }

        /// <summary>Gets the display name.</summary>
        public string Name { get; }

        /// <summary>Gets the duration or charge text.</summary>
        public string DurationText { get; }

        /// <summary>Gets the effect description.</summary>
        public string Description { get; }

        /// <summary>Gets the tone ("Buff" or "Debuff").</summary>
        public string Tone { get; }
    }
}
