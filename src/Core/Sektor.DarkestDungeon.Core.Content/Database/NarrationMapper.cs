using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Campaign;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Maps raw narration content into domain narration entries.</summary>
    public static class NarrationMapper
    {
        /// <summary>
        /// Converts the raw narration data into a dictionary of narration entries keyed by id.
        /// </summary>
        /// <param name="jsonNarration">The raw narration data loaded from the content file.</param>
        /// <returns>The narration entries keyed by id.</returns>
        public static Dictionary<string, NarrationEntry> Parse(JsonNarration jsonNarration)
        {
            var narration = new Dictionary<string, NarrationEntry>();

            foreach (var jsonNarrationEntry in jsonNarration.entries)
            {
                NarrationEntry narrationEntry = new NarrationEntry();
                narrationEntry.Id = jsonNarrationEntry.id;
                narrationEntry.Chance = jsonNarrationEntry.chance;
                narrationEntry.Tone = jsonNarrationEntry.tone;

                foreach (var jsonAudioEvent in jsonNarrationEntry.audio_events)
                {
                    NarrationAudioEvent audioEvent = new NarrationAudioEvent();
                    audioEvent.QueueOnlyOnEmpty = jsonAudioEvent.queue_only_on_empty;
                    audioEvent.QueueWhilePlaying = jsonAudioEvent.queue_while_audio_playing;
                    audioEvent.AudioEvent = jsonAudioEvent.audio_event;
                    audioEvent.Chance = jsonAudioEvent.chance;
                    audioEvent.Priority = jsonAudioEvent.priority;
                    audioEvent.MaxRaidOccurrences = jsonAudioEvent.max_raid_occurrences;
                    audioEvent.MaxTownVisitOccurrences = jsonAudioEvent.max_town_visit_occurrences;
                    audioEvent.MaxCampaignOccurrences = jsonAudioEvent.max_campaign_occurrences;
                    audioEvent.Filter = jsonAudioEvent.filter;
                    audioEvent.CheckAllTags = jsonAudioEvent.check_all_tags;
                    audioEvent.Tags = jsonAudioEvent.tags;
                    narrationEntry.AudioEvents.Add(audioEvent);
                }

                narration.Add(narrationEntry.Id, narrationEntry);
            }

            return narration;
        }
    }
}
