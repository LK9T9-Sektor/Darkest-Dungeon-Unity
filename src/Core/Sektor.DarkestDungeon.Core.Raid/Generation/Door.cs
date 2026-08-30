namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A door prop connecting a room to a hallway.</summary>
    public class Door : Prop
    {
        /// <summary>Gets or sets the target area identifier (the hallway id).</summary>
        public string TargetArea { get; set; }

        /// <summary>Gets or sets the direction of the door.</summary>
        public Direction Direction { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Door"/> class.</summary>
        public Door()
        {
            Type = AreaType.Door;
        }

        /// <summary>Initializes a new instance of the <see cref="Door"/> class.</summary>
        /// <param name="areaId">The room id.</param>
        /// <param name="targetAreaId">The hallway id.</param>
        /// <param name="direction">The door direction.</param>
        public Door(string areaId, string targetAreaId, Direction direction) : this()
        {
            StringId = areaId + targetAreaId;
            TargetArea = targetAreaId;
            Direction = direction;
        }
    }
}