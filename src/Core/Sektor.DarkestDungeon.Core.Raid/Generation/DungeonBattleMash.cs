using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A battle encounter pool of a specific dungeon difficulty (mash).</summary>
    public class DungeonBattleMash
    {
        /// <summary>Gets or sets the difficulty mash id (1-based).</summary>
        public int MashId { get; set; }

        /// <summary>Gets or sets the weighted hallway encounter pool.</summary>
        public List<DungeonBattleEncounter> HallEncounters { get; set; }

        /// <summary>Gets or sets the weighted room encounter pool.</summary>
        public List<DungeonBattleEncounter> RoomEncounters { get; set; }

        /// <summary>Gets or sets the weighted boss encounter pool.</summary>
        public List<DungeonBattleEncounter> BossEncounters { get; set; }

        /// <summary>Gets or sets the weighted stall encounter pool.</summary>
        public List<DungeonBattleEncounter> StallEncounters { get; set; }

        /// <summary>Gets or sets the named encounter pools (keyed by name).</summary>
        public Dictionary<string, List<DungeonBattleEncounter>> NamedEncounters { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DungeonBattleMash"/> class.</summary>
        public DungeonBattleMash()
        {
            HallEncounters = new List<DungeonBattleEncounter>();
            RoomEncounters = new List<DungeonBattleEncounter>();
            BossEncounters = new List<DungeonBattleEncounter>();
            StallEncounters = new List<DungeonBattleEncounter>();
            NamedEncounters = new Dictionary<string, List<DungeonBattleEncounter>>();
        }
    }
}