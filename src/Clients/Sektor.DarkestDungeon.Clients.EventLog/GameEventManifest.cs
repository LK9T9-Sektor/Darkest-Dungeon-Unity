using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>The <c>manifest.json</c> of one recorded run (spec §2).</summary>
    /// <param name="SchemaVersion">The event schema version of the recording.</param>
    /// <param name="StartedAt">UTC start of the recording.</param>
    /// <param name="Seed">The run seed, when known.</param>
    /// <param name="Notes">Free-form notes describing the recorded scenario.</param>
    /// <param name="Files">The JSONL files present in the run directory.</param>
    public sealed record GameEventManifest(
        string SchemaVersion,
        DateTime StartedAt,
        long? Seed,
        string? Notes,
        IReadOnlyList<string> Files);
}
