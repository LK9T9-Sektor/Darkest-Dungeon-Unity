using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A generated dungeon room with its doors.</summary>
    public class DungeonRoom : Area
    {
        /// <summary>Gets the number of connections (doors) of the room.</summary>
        public int Connections { get { return Doors.Count; } }

        /// <summary>Gets or sets the minimum path distance from the entrance room (for quest-goal placement).</summary>
        public int MinPath { get; set; }

        /// <summary>Gets the doors of the room.</summary>
        public List<Door> Doors { get; }

        /// <summary>Initializes a new instance of the <see cref="DungeonRoom"/> class.</summary>
        /// <param name="id">The room id.</param>
        /// <param name="gridX">The final grid X coordinate.</param>
        /// <param name="gridY">The final grid Y coordinate.</param>
        public DungeonRoom(string id, int gridX, int gridY)
        {
            Id = id;
            GridX = gridX;
            GridY = gridY;
            Type = AreaType.Empty;
            Knowledge = Knowledge.Hidden;
            TextureId = "";
            Doors = new List<Door>();
        }
    }
}