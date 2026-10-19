using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>An atomic stat change of a unit (spec §4.2).</summary>
    /// <param name="HeroId">The unit id the stat belongs to.</param>
    /// <param name="Stat">The stat id (hp, stress, ...).</param>
    /// <param name="OldValue">The value before the change.</param>
    /// <param name="NewValue">The value after the change.</param>
    /// <param name="Source">What caused the change (attack, heal, ...).</param>
    /// <param name="Invariant">The invariant the change must satisfy (documentation for consumers).</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">The roll behind the change, or <c>null</c> when not instrumented.</param>
    public sealed record HeroStatChanged(
        string HeroId,
        string Stat,
        int OldValue,
        int NewValue,
        string Source,
        string? Invariant = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
