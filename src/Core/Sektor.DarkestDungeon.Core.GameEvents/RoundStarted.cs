using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A new combat round started.</summary>
    /// <param name="Round">The new round number.</param>
    /// <param name="OrderedUnitIds">The full turn order for the round.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">The initiative roll, or <c>null</c> when not instrumented.</param>
    public sealed record RoundStarted(
        int Round,
        IReadOnlyList<string>? OrderedUnitIds = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
