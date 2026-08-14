namespace Sektor.DarkestDungeon.Core.Content.Raid
{
    /// <summary>Identifies the kind of encounter a dungeon area holds.</summary>
    public enum AreaType
    {
        /// <summary>An empty area.</summary>
        Empty,
        /// <summary>The dungeon entrance.</summary>
        Entrance,
        /// <summary>A treasure area.</summary>
        Tresure,
        /// <summary>A curio area.</summary>
        Curio,
        /// <summary>A boss area.</summary>
        Boss,
        /// <summary>A battle area.</summary>
        Battle,
        /// <summary>A trap area.</summary>
        Trap,
        /// <summary>A hunger event area.</summary>
        Hunger,
        /// <summary>An obstacle area.</summary>
        Obstacle,
        /// <summary>A door area.</summary>
        Door,
        /// <summary>A battle curio area.</summary>
        BattleCurio,
        /// <summary>A battle treasure area.</summary>
        BattleTresure,
    }
}
