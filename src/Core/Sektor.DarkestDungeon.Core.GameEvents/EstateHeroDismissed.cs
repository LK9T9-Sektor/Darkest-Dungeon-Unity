namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A hero was dismissed from the estate roster.</summary>
    /// <param name="HeroId">The dismissed hero id.</param>
    /// <param name="Class">The hero class id.</param>
    /// <param name="Name">The hero name.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateHeroDismissed(
        string HeroId,
        string Class,
        string Name,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
