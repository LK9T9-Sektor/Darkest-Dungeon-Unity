using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Character
{
    /// <summary>A hero quirk (positive or negative) loaded from content.</summary>
    public class Quirk
    {
        /// <summary>Gets or sets the quirk identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the classification (physical / combat / etc.).</summary>
        public string Classification { get; set; }

        /// <summary>Gets or sets a value indicating whether the effect text is explicit (buffs) rather than a localized description.</summary>
        public bool ShowExplicitDescription { get; set; }

        /// <summary>Gets or sets a value indicating whether the quirk is a positive one.</summary>
        public bool IsPositive { get; set; }

        /// <summary>Gets or sets a value indicating whether the quirk is a disease.</summary>
        public bool IsDisease { get; set; }

        /// <summary>Gets or sets a value indicating whether the hero keeps the loot when this quirk is present.</summary>
        public bool KeepLoot { get; set; }

        /// <summary>Gets or sets the curio tag the quirk reacts to.</summary>
        public string CurioTag { get; set; }

        /// <summary>Gets or sets the chance to react to the curio tag.</summary>
        public float CurioTagChance { get; set; }

        /// <summary>Gets the quirk ids incompatible with this one.</summary>
        public List<string> IncompatibleQuirks { get; } = new List<string>();

        /// <summary>Gets the buff ids applied by this quirk.</summary>
        public List<string> Buffs { get; } = new List<string>();
    }
}