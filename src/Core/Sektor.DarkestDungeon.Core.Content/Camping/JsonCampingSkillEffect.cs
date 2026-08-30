using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Camping
{
    /// <summary>A single camping skill effect.</summary>
    public class JsonCampingSkillEffect
    {
        /// <summary>Gets or sets the selection mode ("individual" or party-wide).</summary>
        public string selection { get; set; }

        /// <summary>Gets or sets the target requirements (opaque strings or objects).</summary>
        public List<object> requirements { get; set; }

        /// <summary>Gets or sets the effect chance.</summary>
        public JsonCampingChance chance { get; set; }

        /// <summary>Gets or sets the effect type.</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the effect sub type.</summary>
        public string sub_type { get; set; }

        /// <summary>Gets or sets the effect amount.</summary>
        public float amount { get; set; }
    }
}