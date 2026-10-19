namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="StateSnapshot.Context"/> (why the snapshot was taken).</summary>
    public static class StateSnapshotContexts
    {
        /// <summary>Taken when a battle starts.</summary>
        public const string BattleStart = "battleStart";

        /// <summary>Taken after a combat turn resolves.</summary>
        public const string TurnEnd = "turnEnd";

        /// <summary>Taken when the party enters a room.</summary>
        public const string RoomEntered = "roomEntered";

        /// <summary>Taken when the inventory is opened.</summary>
        public const string InventoryOpened = "inventoryOpened";
    }
}
