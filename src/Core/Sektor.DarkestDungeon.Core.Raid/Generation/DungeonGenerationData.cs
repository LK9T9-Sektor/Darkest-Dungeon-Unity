namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Dungeon generation parameters from the <c>Data/Mechanics/MapGenerator</c> DSL: room/corridor
    /// counts, grid size, spacing and the min/max ranges of every populated feature.
    /// </summary>
    public class DungeonGenerationData
    {
        /// <summary>Gets or sets the quest length key ("short"/"medium"/"long").</summary>
        public string Length { get; set; }

        /// <summary>Gets or sets the quest type key.</summary>
        public string QuestType { get; set; }

        /// <summary>Gets or sets the dungeon type key.</summary>
        public string Dungeon { get; set; }

        /// <summary>Gets or sets the base number of rooms.</summary>
        public int BaseRoomNumber { get; set; }

        /// <summary>Gets or sets the base number of corridors.</summary>
        public int BaseCorridorNumber { get; set; }

        /// <summary>Gets or sets the grid width.</summary>
        public int GridSizeX { get; set; }

        /// <summary>Gets or sets the grid height.</summary>
        public int GridSizeY { get; set; }

        /// <summary>Gets or sets the corridor spacing (hall sectors between two rooms).</summary>
        public int Spacing { get; set; }

        /// <summary>Gets or sets the number of goal rooms.</summary>
        public int GoalRoomNumber { get; set; }

        /// <summary>Gets or sets the minimum final distance of the goal room.</summary>
        public int MinFinalDistance { get; set; }

        /// <summary>Gets or sets the hallway battle count range (min, max).</summary>
        public int HallwayBattleMin { get; set; }

        /// <summary>Gets or sets the hallway battle count range (min, max).</summary>
        public int HallwayBattleMax { get; set; }

        /// <summary>Gets or sets the hallway trap count range (min, max).</summary>
        public int HallwayTrapMin { get; set; }

        /// <summary>Gets or sets the hallway trap count range (min, max).</summary>
        public int HallwayTrapMax { get; set; }

        /// <summary>Gets or sets the hallway obstacle count range (min, max).</summary>
        public int HallwayObstacleMin { get; set; }

        /// <summary>Gets or sets the hallway obstacle count range (min, max).</summary>
        public int HallwayObstacleMax { get; set; }

        /// <summary>Gets or sets the hallway curio count range (min, max).</summary>
        public int HallwayCurioMin { get; set; }

        /// <summary>Gets or sets the hallway curio count range (min, max).</summary>
        public int HallwayCurioMax { get; set; }

        /// <summary>Gets or sets the hallway hunger count range (min, max).</summary>
        public int HallwayHungerMin { get; set; }

        /// <summary>Gets or sets the hallway hunger count range (min, max).</summary>
        public int HallwayHungerMax { get; set; }

        /// <summary>Gets or sets the total room battle count range (min, max).</summary>
        public int TotalRoomBattleMin { get; set; }

        /// <summary>Gets or sets the total room battle count range (min, max).</summary>
        public int TotalRoomBattleMax { get; set; }

        /// <summary>Gets or sets the room guarded curio count range (min, max).</summary>
        public int RoomGuardedCurioMin { get; set; }

        /// <summary>Gets or sets the room guarded curio count range (min, max).</summary>
        public int RoomGuardedCurioMax { get; set; }

        /// <summary>Gets or sets the room guarded treasure count range (min, max).</summary>
        public int RoomGuardedTresureMin { get; set; }

        /// <summary>Gets or sets the room guarded treasure count range (min, max).</summary>
        public int RoomGuardedTresureMax { get; set; }
    }
}