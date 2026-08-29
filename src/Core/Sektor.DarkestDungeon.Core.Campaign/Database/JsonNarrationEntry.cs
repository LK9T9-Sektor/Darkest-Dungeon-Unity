using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>A raw narration entry as loaded from the content file.</summary>
    public class JsonNarrationEntry
    {
        /// <summary>Gets the identifier of the narration entry.</summary>
        public string id { get; set; }

        /// <summary>Gets the tone of the narration.</summary>
        public string tone { get; set; }

        /// <summary>Gets the selection chance of the narration.</summary>
        public float chance { get; set; }

        /// <summary>Gets the audio events of the narration.</summary>
        public List<JsonNarrationEntryAudioEvent> audio_events { get; set; }
    }
}
