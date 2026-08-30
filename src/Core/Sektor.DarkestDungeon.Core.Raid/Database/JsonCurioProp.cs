using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Database
{
    /// <summary>A curio prop (trap or scenic obstacle) definition.</summary>
    public class JsonCurioProp
    {
        /// <summary>Gets or sets the prop name.</summary>
        public string name { get; set; }

        /// <summary>Gets or sets the effects on successful interaction.</summary>
        public List<string> success_effects { get; set; }

        /// <summary>Gets or sets the effects on failed interaction.</summary>
        public List<string> fail_effects { get; set; }

        /// <summary>Gets or sets the health fraction change on failed interaction.</summary>
        public double health { get; set; }

        /// <summary>Gets or sets the torchlight change on failed interaction.</summary>
        public double torchlight { get; set; }

        /// <summary>Gets or sets whether an ancestor talk is triggered.</summary>
        public bool ancestor_talk { get; set; }

        /// <summary>Gets or sets the level variations (traps only).</summary>
        public List<JsonCurioPropVariation> difficulty_variations { get; set; }
    }
}