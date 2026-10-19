using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>The torch value changed (spec §4.5).</summary>
    /// <param name="OldValue">The torch value before the change.</param>
    /// <param name="NewValue">The torch value after the change.</param>
    /// <param name="Reason">Why it changed (corridorStep, itemUsed, snuffed, ...).</param>
    /// <param name="Input">The cause, or <c>null</c> (passive decay).</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record TorchChanged(
        int OldValue,
        int NewValue,
        string Reason,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Torch, Input, Rng);
}
