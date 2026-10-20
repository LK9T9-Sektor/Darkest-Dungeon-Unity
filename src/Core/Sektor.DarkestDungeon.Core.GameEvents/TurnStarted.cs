using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A unit started its combat turn.</summary>
    /// <param name="ActorId">The unit whose turn started.</param>
    /// <param name="Round">The current round number.</param>
    /// <param name="Turn">The turn kind (for example <c>Hero</c>).</param>
    /// <param name="OrderedUnitIds">The remaining turn order after removing the actor.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record TurnStarted(
        string ActorId,
        int Round,
        string? Turn = null,
        IReadOnlyList<string>? OrderedUnitIds = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
