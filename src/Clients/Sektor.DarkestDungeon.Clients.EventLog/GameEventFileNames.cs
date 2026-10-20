namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>Well-known file names of one recorded run directory (spec §1).</summary>
    public static class GameEventFileNames
    {
        /// <summary>The manifest file with the schema version and the list of event files.</summary>
        public const string Manifest = "manifest.json";

        /// <summary>The suffix of the per-subsystem JSONL files.</summary>
        public const string SessionSuffix = "_session.jsonl";

        /// <summary>Builds the JSONL file name of one subsystem.</summary>
        /// <param name="subsystem">The subsystem key.</param>
        /// <returns>The <c>&lt;subsystem&gt;_session.jsonl</c> file name.</returns>
        public static string SessionFile(string subsystem) => subsystem + SessionSuffix;
    }
}
