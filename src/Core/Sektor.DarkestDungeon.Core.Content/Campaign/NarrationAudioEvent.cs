using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Campaign
{
    /// <summary>A single narration audio event with its occurrence limits and tags.</summary>
    public class NarrationAudioEvent
    {
        /// <summary>Gets or sets a value indicating whether the event queues only when empty.</summary>
        public bool QueueOnlyOnEmpty { get; set; }

        /// <summary>Gets or sets a value indicating whether the event queues while audio plays.</summary>
        public bool QueueWhilePlaying { get; set; }

        /// <summary>Gets or sets the audio event id.</summary>
        public string AudioEvent { get; set; }

        /// <summary>Gets or sets the selection chance.</summary>
        public float Chance { get; set; }

        /// <summary>Gets or sets the selection priority.</summary>
        public float Priority { get; set; }

        /// <summary>Gets or sets the maximum occurrences per raid.</summary>
        public int MaxRaidOccurrences { get; set; }

        /// <summary>Gets or sets the maximum occurrences per town visit.</summary>
        public int MaxTownVisitOccurrences { get; set; }

        /// <summary>Gets or sets the maximum occurrences per campaign.</summary>
        public int MaxCampaignOccurrences { get; set; }

        /// <summary>Gets or sets the filter of the event.</summary>
        public string Filter { get; set; }

        /// <summary>Gets or sets a value indicating whether all tags must match.</summary>
        public bool CheckAllTags { get; set; }

        /// <summary>Gets the required tags of the event.</summary>
        public List<string> Tags { get; set; }

        /// <summary>Initializes a new instance of the <see cref="NarrationAudioEvent"/> class.</summary>
        public NarrationAudioEvent()
        {
            Tags = new List<string>();
        }

        /// <summary>
        /// Determines whether the event can play in the given place given the recorded
        /// occurrence counts and the current tags.
        /// </summary>
        /// <param name="narrationPlace">The place the narration would play in.</param>
        /// <param name="raidOccurrences">The recorded occurrence counts for the raid place.</param>
        /// <param name="townOccurrences">The recorded occurrence counts for the town place.</param>
        /// <param name="campaignOccurrences">The recorded occurrence counts for the campaign place.</param>
        /// <param name="tags">The tags available at the moment of selection.</param>
        /// <returns>True when the event may play, otherwise false.</returns>
        public bool IsPossible(NarrationPlace narrationPlace,
            IReadOnlyDictionary<string, int> raidOccurrences,
            IReadOnlyDictionary<string, int> townOccurrences,
            IReadOnlyDictionary<string, int> campaignOccurrences,
            params string[] tags)
        {
            switch (narrationPlace)
            {
                case NarrationPlace.Campaign:
                    if (MaxCampaignOccurrences > 0 && Exceeds(campaignOccurrences, MaxCampaignOccurrences))
                        return false;
                    break;
                case NarrationPlace.Raid:
                    if (MaxRaidOccurrences > 0 && Exceeds(raidOccurrences, MaxRaidOccurrences))
                        return false;
                    goto case NarrationPlace.Campaign;
                case NarrationPlace.Town:
                    if (MaxTownVisitOccurrences > 0 && Exceeds(townOccurrences, MaxTownVisitOccurrences))
                        return false;
                    goto case NarrationPlace.Campaign;
            }

            if (Tags.Count > 0)
            {
                if (CheckAllTags)
                {
                    for (int i = 0; i < Tags.Count; i++)
                    {
                        if (tags.Length == 0)
                            return false;

                        for (int j = 0; j < tags.Length; j++)
                        {
                            if (tags[j] == Tags[i])
                                break;

                            if (j == tags.Length - 1)
                                return false;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tags.Length; i++)
                        if (Tags.Contains(tags[i]))
                            return true;

                    return false;
                }
            }
            return true;
        }

        private bool Exceeds(IReadOnlyDictionary<string, int> occurrences, int max)
        {
            int count;
            return occurrences != null && occurrences.TryGetValue(AudioEvent, out count) && count >= max;
        }
    }
}
