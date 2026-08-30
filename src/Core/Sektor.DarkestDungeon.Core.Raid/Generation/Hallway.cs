using System.Collections.Generic;
using System.Linq;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A generated hallway connecting two rooms, made of hall sectors.</summary>
    public class Hallway
    {
        /// <summary>Gets or sets the hallway id.</summary>
        public string Id { get; set; }

        /// <summary>Gets the hall sectors of the hallway.</summary>
        public List<HallSector> Halls { get; }

        /// <summary>Gets or sets the first room.</summary>
        public DungeonRoom RoomA { get; set; }

        /// <summary>Gets or sets the second room.</summary>
        public DungeonRoom RoomB { get; set; }

        /// <summary>Gets the direction from room A (toward this hallway).</summary>
        public Direction DirectionFromA
        {
            get
            {
                var targetDoor = RoomA.Doors.FirstOrDefault(door => door.TargetArea == Id);
                return targetDoor != null ? targetDoor.Direction : Direction.Right;
            }
        }

        /// <summary>Gets the direction from room B (toward this hallway).</summary>
        public Direction DirectionFromB
        {
            get
            {
                var targetDoor = RoomB.Doors.FirstOrDefault(door => door.TargetArea == Id);
                return targetDoor != null ? targetDoor.Direction : Direction.Right;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="Hallway"/> class.</summary>
        /// <param name="id">The hallway id.</param>
        public Hallway(string id)
        {
            Id = id;
            Halls = new List<HallSector>();
        }
    }
}