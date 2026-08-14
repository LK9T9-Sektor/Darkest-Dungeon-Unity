using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Campaign
{
    /// <summary>A single narration entry with its audio events.</summary>
    public class NarrationEntry
    {
        /// <summary>Gets or sets the identifier of the narration entry.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the tone of the narration.</summary>
        public string Tone { get; set; }

        /// <summary>Gets or sets the selection chance of the narration.</summary>
        public float Chance { get; set; }

        /// <summary>Gets the audio events of the narration.</summary>
        public List<NarrationAudioEvent> AudioEvents { get; set; }

        /// <summary>Initializes a new instance of the <see cref="NarrationEntry"/> class.</summary>
        public NarrationEntry()
        {
            AudioEvents = new List<NarrationAudioEvent>();
        }
    }
}
