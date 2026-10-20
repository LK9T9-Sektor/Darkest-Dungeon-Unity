namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A unit finished its combat turn.</summary>
    /// <param name="ActorId">The unit whose turn ended.</param>
    /// <param name="Round">The current round number.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record TurnEnded(
        string ActorId,
        int Round,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
