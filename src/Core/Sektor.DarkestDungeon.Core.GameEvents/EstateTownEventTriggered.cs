namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A town event is active on estate entry.</summary>
    /// <param name="TownEventId">The town event id.</param>
    /// <param name="Tone">The town event tone.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateTownEventTriggered(
        string TownEventId,
        string Tone,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
