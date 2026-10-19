using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A previously unknown room was discovered (spec §4.3).</summary>
    /// <param name="RoomId">The room id.</param>
    /// <param name="RoomType">The room type (battle, treasure, entrance, ...).</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record RoomDiscovered(
        string RoomId,
        string RoomType,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Map, Input, Rng);
}
