using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Database
{
    /// <summary>A curio prop variation for a given dungeon level.</summary>
    public class JsonCurioPropVariation
    {
        /// <summary>Gets or sets the dungeon level the variation applies to.</summary>
        public int level { get; set; }

        /// <summary>Gets or sets the effects on successful interaction.</summary>
        public List<string> success_effects { get; set; }

        /// <summary>Gets or sets the effects on failed interaction.</summary>
        public List<string> fail_effects { get; set; }

        /// <summary>Gets or sets the health fraction change on failed interaction.</summary>
        public double health { get; set; }

        /// <summary>Gets or sets the torchlight change on failed interaction.</summary>
        public double torchlight { get; set; }
    }
}