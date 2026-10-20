namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A battle finished.</summary>
    /// <param name="Winner">The winning side (see <see cref="BattleWinners"/>).</param>
    /// <param name="Rounds">Number of completed rounds.</param>
    /// <param name="Retreat">Whether the party retreated instead of winning or losing.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record BattleEnded(
        string Winner,
        int Rounds,
        bool Retreat = false,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
