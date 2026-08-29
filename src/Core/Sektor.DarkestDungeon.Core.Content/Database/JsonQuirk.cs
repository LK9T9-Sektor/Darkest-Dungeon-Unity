using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Raw quirk entry from the JsonQuirks.json content file.</summary>
    public class JsonQuirk
    {
        /// <summary>Gets or sets the quirk identifier.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets a value indicating whether the effect text is explicit.</summary>
        public bool show_explicit_description { get; set; }

        /// <summary>Gets or sets a value indicating whether the quirk is positive.</summary>
        public bool is_positive { get; set; }

        /// <summary>Gets or sets a value indicating whether the quirk is a disease.</summary>
        public bool is_disease { get; set; }

        /// <summary>Gets or sets the quirk classification.</summary>
        public string classification { get; set; }

        /// <summary>Gets or sets the incompatible quirk ids.</summary>
        public List<string> incompatible_quirks { get; set; }

        /// <summary>Gets or sets the curio tag.</summary>
        public string curio_tag { get; set; }

        /// <summary>Gets or sets the curio tag reaction chance.</summary>
        public float curio_tag_chance { get; set; }

        /// <summary>Gets or sets a value indicating whether the hero keeps loot.</summary>
        public bool keep_loot { get; set; }

        /// <summary>Gets or sets the buff ids applied by the quirk.</summary>
        public List<string> buffs { get; set; }
    }
}