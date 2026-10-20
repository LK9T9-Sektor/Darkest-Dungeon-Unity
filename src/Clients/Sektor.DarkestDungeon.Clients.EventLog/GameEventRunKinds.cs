namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>Run kinds recorded in <c>manifest.json</c>; old manifests without a kind are raids.</summary>
    public static class GameEventRunKinds
    {
        /// <summary>A raid/dungeon run.</summary>
        public const string Raid = "raid";

        /// <summary>An estate/town run.</summary>
        public const string Estate = "estate";
    }
}
