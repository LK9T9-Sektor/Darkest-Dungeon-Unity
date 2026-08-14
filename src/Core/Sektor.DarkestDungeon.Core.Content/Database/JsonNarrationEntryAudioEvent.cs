using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>A raw narration audio event as loaded from the content file.</summary>
    public class JsonNarrationEntryAudioEvent
    {
        /// <summary>Gets a value indicating whether the event queues only when empty.</summary>
        public bool queue_only_on_empty { get; set; }

        /// <summary>Gets a value indicating whether the event queues while audio plays.</summary>
        public bool queue_while_audio_playing { get; set; }

        /// <summary>Gets the audio event id.</summary>
        public string audio_event { get; set; }

        /// <summary>Gets the selection chance.</summary>
        public float chance { get; set; }

        /// <summary>Gets the selection priority.</summary>
        public float priority { get; set; }

        /// <summary>Gets the maximum occurrences per raid.</summary>
        public int max_raid_occurrences { get; set; }

        /// <summary>Gets the maximum occurrences per town visit.</summary>
        public int max_town_visit_occurrences { get; set; }

        /// <summary>Gets the maximum occurrences per campaign.</summary>
        public int max_campaign_occurrences { get; set; }

        /// <summary>Gets the filter of the event.</summary>
        public string filter { get; set; }

        /// <summary>Gets a value indicating whether all tags must match.</summary>
        public bool check_all_tags { get; set; }

        /// <summary>Gets the required tags of the event.</summary>
        public List<string> tags { get; set; }
    }
}
