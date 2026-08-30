using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A generated dungeon: rooms, hallways, grid size and the populated feature counts.</summary>
    public class Dungeon
    {
        /// <summary>Gets or sets the dungeon name (region id).</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the final grid width (1 + (x-1)*7).</summary>
        public int GridSizeX { get; set; }

        /// <summary>Gets or sets the final grid height (1 + (y-1)*7).</summary>
        public int GridSizeY { get; set; }

        /// <summary>Gets or sets the starting room id.</summary>
        public string StartingRoomId { get; set; }

        /// <summary>Gets or sets the total number of room battles.</summary>
        public int TotalRoomBattles { get; set; }

        /// <summary>Gets or sets the number of guarded curios in rooms.</summary>
        public int RoomGuardedCurio { get; set; }

        /// <summary>Gets or sets the number of guarded treasures in rooms.</summary>
        public int RoomGuardedTresure { get; set; }

        /// <summary>Gets or sets the number of hallway battles.</summary>
        public int HallwayBattles { get; set; }

        /// <summary>Gets or sets the number of hallway traps.</summary>
        public int HallwayTraps { get; set; }

        /// <summary>Gets or sets the number of hallway obstacles.</summary>
        public int HallwayObstacles { get; set; }

        /// <summary>Gets or sets the number of hallway curios.</summary>
        public int HallwayCurios { get; set; }

        /// <summary>Gets or sets the number of hallway hunger events.</summary>
        public int HallwayHunger { get; set; }

        /// <summary>Gets the rooms by id.</summary>
        public Dictionary<string, DungeonRoom> Rooms { get; }

        /// <summary>Gets the hallways by id.</summary>
        public Dictionary<string, Hallway> Hallways { get; }

        /// <summary>Initializes a new instance of the <see cref="Dungeon"/> class.</summary>
        public Dungeon()
        {
            Rooms = new Dictionary<string, DungeonRoom>();
            Hallways = new Dictionary<string, Hallway>();
        }
    }
}