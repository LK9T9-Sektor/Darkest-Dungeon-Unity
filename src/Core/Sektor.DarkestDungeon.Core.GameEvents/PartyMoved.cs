using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>The party moved from one map location to another (spec §4.3).</summary>
    /// <param name="FromId">The source room or hall-sector id.</param>
    /// <param name="ToId">The destination room or hall-sector id.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record PartyMoved(
        string FromId,
        string ToId,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Map, Input, Rng);
}
