using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A weighted prop encounter (curio, trap, obstacle) by prop name.</summary>
    public class DungeonPropsEncounter : IProportionValue
    {
        /// <inheritdoc/>
        public int Chance { get; set; }

        /// <summary>Gets the prop identifier (curio/trap/obstacle name).</summary>
        public string PropName { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="DungeonPropsEncounter"/> class.</summary>
        /// <param name="chance">The proportional selection chance.</param>
        /// <param name="prop">The prop identifier.</param>
        public DungeonPropsEncounter(int chance, string prop)
        {
            Chance = chance;
            PropName = prop;
        }
    }
}