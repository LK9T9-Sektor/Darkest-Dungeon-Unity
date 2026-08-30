using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A battle encounter: the set of monster ids spawned on an area, with cleared state.</summary>
    public class BattleEncounter
    {
        /// <summary>Gets the monster ids of the encounter.</summary>
        public List<string> Monsters { get; }

        /// <summary>Gets or sets a value indicating whether the encounter was cleared.</summary>
        public bool Cleared { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BattleEncounter"/> class.</summary>
        public BattleEncounter()
        {
            Monsters = new List<string>();
        }

        /// <summary>Initializes a new instance of the <see cref="BattleEncounter"/> class.</summary>
        /// <param name="monsterNames">The monster ids of the encounter.</param>
        public BattleEncounter(IEnumerable<string> monsterNames)
        {
            Monsters = new List<string>(monsterNames);
        }
    }
}