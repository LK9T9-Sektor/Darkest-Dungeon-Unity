namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A combat round ended.</summary>
    /// <param name="Round">The round that ended.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record RoundEnded(
        int Round,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
