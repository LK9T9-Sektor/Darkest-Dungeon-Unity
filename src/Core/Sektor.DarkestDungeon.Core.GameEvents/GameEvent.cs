using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// Base of every recorded game event (see <c>docs\SPEC_GAME_EVENTS.md</c>). One JSONL line =
    /// one event; <see cref="EventId"/>, <see cref="Timestamp"/> and <see cref="Sequence"/> are
    /// stamped by <see cref="GameEventBus.Publish{T}"/> - event records are only ever published
    /// through the bus.
    /// </summary>
    /// <param name="EventId">Unique id stamped on publish.</param>
    /// <param name="Timestamp">UTC stamp stamped on publish.</param>
    /// <param name="Sequence">Monotonic publish counter stamped on publish (0 = not yet stamped).</param>
    /// <param name="Subsystem">Wire key grouping the log file (see <see cref="SubsystemNames"/>).</param>
    /// <param name="Input">What caused the event, or <c>null</c> when not instrumented.</param>
    /// <param name="Rng">The probability roll behind the event, or <c>null</c> when not instrumented.</param>
    public abstract record GameEvent(
        Guid EventId,
        DateTime Timestamp,
        long Sequence,
        string Subsystem,
        GameInput? Input,
        GameRng? Rng);
}
