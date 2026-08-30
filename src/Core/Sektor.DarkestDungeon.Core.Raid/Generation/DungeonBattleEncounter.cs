using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A weighted battle encounter: a monster set selected by proportional chance.</summary>
    public class DungeonBattleEncounter : IProportionValue
    {
        /// <inheritdoc/>
        public int Chance { get; set; }

        /// <summary>Gets the monster ids of the encounter.</summary>
        public List<string> MonsterSet { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="DungeonBattleEncounter"/> class.</summary>
        public DungeonBattleEncounter()
        {
            MonsterSet = new List<string>();
        }

        /// <summary>Initializes a new instance of the <see cref="DungeonBattleEncounter"/> class.</summary>
        /// <param name="chance">The proportional selection chance.</param>
        /// <param name="set">The monster ids.</param>
        public DungeonBattleEncounter(int chance, List<string> set) : this()
        {
            Chance = chance;
            MonsterSet = set;
        }
    }
}