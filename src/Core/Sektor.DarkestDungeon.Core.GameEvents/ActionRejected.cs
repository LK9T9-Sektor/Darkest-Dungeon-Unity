using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A player action was rejected - negative test case for consumers (spec §5).</summary>
    /// <param name="Reason">Why the action was rejected.</param>
    /// <param name="Subsystem">The subsystem owning the action (combat/map/inventory).</param>
    /// <param name="Input">The rejected input (required - the point of the event).</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record ActionRejected(
        string Reason,
        string Subsystem,
        GameInput Input,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, Subsystem, Input, Rng);
}
