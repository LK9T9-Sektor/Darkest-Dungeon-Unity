using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>An audio event that actually played (music, SFX, ambience, or narration).</summary>
    /// <param name="EventPath">The exact FMOD event path requested at runtime.</param>
    /// <param name="Kind">The audio kind (see <see cref="AudioEventKinds"/>).</param>
    /// <param name="Action">The action performed (see <see cref="AudioActions"/>).</param>
    /// <param name="Volume">The requested volume, when known.</param>
    /// <param name="Position">The world position for 3D events, when known.</param>
    /// <param name="Parameters">FMOD parameters set before starting, when known.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">The chance roll for narration, or <c>null</c>.</param>
    public sealed record AudioPlayed(
        string EventPath,
        string Kind,
        string Action,
        float? Volume = null,
        AudioPosition? Position = null,
        IReadOnlyList<AudioParameter>? Parameters = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Audio, Input, Rng);
}
