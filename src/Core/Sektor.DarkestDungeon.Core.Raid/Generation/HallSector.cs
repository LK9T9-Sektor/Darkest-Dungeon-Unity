namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A single sector of a generated hallway (including its door sectors).</summary>
    public class HallSector : Area
    {
        /// <summary>Gets or sets the parent hallway.</summary>
        public Hallway Hallway { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HallSector"/> class.</summary>
        /// <param name="id">The sector id.</param>
        /// <param name="gridX">The final grid X coordinate.</param>
        /// <param name="gridY">The final grid Y coordinate.</param>
        /// <param name="parentHallway">The parent hallway.</param>
        public HallSector(string id, int gridX, int gridY, Hallway parentHallway)
        {
            Id = id;
            Hallway = parentHallway;
            GridX = gridX;
            GridY = gridY;
            Knowledge = Knowledge.Hidden;
            TextureId = "0";
        }

        /// <summary>Initializes a new instance of the <see cref="HallSector"/> class as a door sector.</summary>
        /// <param name="id">The sector id.</param>
        /// <param name="gridX">The final grid X coordinate.</param>
        /// <param name="gridY">The final grid Y coordinate.</param>
        /// <param name="parentHallway">The parent hallway.</param>
        /// <param name="door">The door prop.</param>
        public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Door door)
        {
            Id = id;
            Hallway = parentHallway;
            GridX = gridX;
            GridY = gridY;
            Knowledge = Knowledge.Hidden;
            TextureId = "0";
            Prop = door;
            Type = AreaType.Door;
        }
    }
}