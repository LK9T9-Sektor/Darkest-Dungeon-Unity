using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A hero overstress trait (affliction or virtue).</summary>
    public class Trait
    {
        /// <summary>Gets or sets the trait identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the overstress type.</summary>
        public OverstressType Type { get; set; }

        /// <summary>Gets the buff ids applied while the trait is active.</summary>
        public List<string> BuffIds { get; } = new List<string>();

        /// <summary>Gets a value indicating whether the trait is an affliction.</summary>
        public bool IsAffliction { get { return Type == OverstressType.Affliction; } }

        /// <summary>Gets a value indicating whether the trait is a virtue.</summary>
        public bool IsVirtue { get { return Type == OverstressType.Virtue; } }
    }
}