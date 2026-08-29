using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>A town event definition with its triggers and effects.</summary>
    public class JsonTownEvent
    {
        /// <summary>Gets or sets the event id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the base chance.</summary>
        public double base_chance { get; set; }

        /// <summary>Gets or sets the additional chance when not rolled.</summary>
        public double per_not_rolled_additional_chance { get; set; }

        /// <summary>Gets or sets the cooldown in town visits.</summary>
        public int cooldown { get; set; }

        /// <summary>Gets or sets the event requirements block (opaque content data).</summary>
        public Dictionary<string, object> requirements { get; set; }

        /// <summary>Gets or sets the town ambience parameter ids.</summary>
        public List<string> town_ambience_paramater_ids { get; set; }

        /// <summary>Gets or sets the tone (good, bad...).</summary>
        public string tone { get; set; }

        /// <summary>Gets or sets the sprite id.</summary>
        public string sprite { get; set; }

        /// <summary>Gets or sets the sprite attachment.</summary>
        public string sprite_attachment { get; set; }

        /// <summary>Gets or sets the event effect data entries.</summary>
        public List<JsonTownEventDataEntry> data { get; set; }
    }
}