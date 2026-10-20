namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire keys of <see cref="GameEvent.Subsystem"/> (one JSONL file per subsystem).</summary>
    public static class SubsystemNames
    {
        /// <summary>Battle events (attacks, stats, skill availability).</summary>
        public const string Combat = "combat";

        /// <summary>Map and movement events (rooms, transitions).</summary>
        public const string Map = "map";

        /// <summary>Inventory events (item deltas).</summary>
        public const string Inventory = "inventory";

        /// <summary>Torch events.</summary>
        public const string Torch = "torch";

        /// <summary>Audio events (music, SFX, ambience, narration).</summary>
        public const string Audio = "audio";

        /// <summary>AI decision events.</summary>
        public const string Ai = "ai";

        /// <summary>Fallback for events published without a subsystem.</summary>
        public const string Unspecified = "unspecified";
    }
}
