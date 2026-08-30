namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>Base of a generated dungeon area (room or hall sector).</summary>
    public abstract class Area
    {
        /// <summary>Gets or sets the area identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the texture variation identifier.</summary>
        public string TextureId { get; set; }

        /// <summary>Gets or sets the grid X coordinate (final 7-step grid).</summary>
        public int GridX { get; set; }

        /// <summary>Gets or sets the grid Y coordinate (final 7-step grid).</summary>
        public int GridY { get; set; }

        /// <summary>Gets or sets the area type.</summary>
        public AreaType Type { get; set; }

        /// <summary>Gets or sets the exploration knowledge.</summary>
        public Knowledge Knowledge { get; set; }

        /// <summary>Gets or sets the prop placed on the area (curio, trap, obstacle or door).</summary>
        public Prop Prop { get; set; }

        /// <summary>Gets or sets the battle encounter of the area.</summary>
        public BattleEncounter BattleEncounter { get; set; }
    }
}