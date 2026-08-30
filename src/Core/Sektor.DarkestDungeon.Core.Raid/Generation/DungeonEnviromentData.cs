using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Environment data of a dungeon region from the <c>Data/Dungeons/*</c> DSL: hall/room
    /// variations, battle mashes (encounter pools) and the weighted prop pools (curios, treasures,
    /// traps, obstacles).
    /// </summary>
    public class DungeonEnviromentData
    {
        /// <summary>Gets or sets the number of hall texture variations.</summary>
        public int HallVariations { get; set; }

        /// <summary>Gets or sets the room texture variation ids.</summary>
        public List<string> RoomVariations { get; set; }

        /// <summary>Gets or sets the battle mashes (encounter pools per difficulty).</summary>
        public List<DungeonBattleMash> BattleMashes { get; set; }

        /// <summary>Gets or sets the weighted hall curio pool.</summary>
        public List<DungeonPropsEncounter> HallCurios { get; set; }

        /// <summary>Gets or sets the weighted room curio pool.</summary>
        public List<DungeonPropsEncounter> RoomCurios { get; set; }

        /// <summary>Gets or sets the weighted room treasure pool.</summary>
        public List<DungeonPropsEncounter> RoomTresures { get; set; }

        /// <summary>Gets or sets the weighted secret treasure pool.</summary>
        public List<DungeonPropsEncounter> SecretTresures { get; set; }

        /// <summary>Gets or sets the weighted trap pool.</summary>
        public List<DungeonPropsEncounter> Traps { get; set; }

        /// <summary>Gets or sets the weighted obstacle pool.</summary>
        public List<DungeonPropsEncounter> Obstacles { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DungeonEnviromentData"/> class.</summary>
        public DungeonEnviromentData()
        {
            RoomVariations = new List<string>();
            BattleMashes = new List<DungeonBattleMash>();
            HallCurios = new List<DungeonPropsEncounter>();
            RoomCurios = new List<DungeonPropsEncounter>();
            RoomTresures = new List<DungeonPropsEncounter>();
            SecretTresures = new List<DungeonPropsEncounter>();
            Traps = new List<DungeonPropsEncounter>();
            Obstacles = new List<DungeonPropsEncounter>();
        }
    }
}