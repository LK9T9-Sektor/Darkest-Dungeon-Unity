using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>One parsed recording: the manifest and every event ordered by <see cref="GameEvent.Sequence"/>.</summary>
    /// <param name="Manifest">The run manifest.</param>
    /// <param name="Events">All events of the run, sorted by sequence (1..N without gaps or duplicates).</param>
    public sealed record GameEventCapture(GameEventManifest Manifest, IReadOnlyList<GameEvent> Events);
}
