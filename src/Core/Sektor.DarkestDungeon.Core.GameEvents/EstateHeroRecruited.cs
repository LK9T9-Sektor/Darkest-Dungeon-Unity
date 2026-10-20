using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A hero was recruited into the estate roster.</summary>
    /// <param name="HeroId">The recruited hero id.</param>
    /// <param name="Class">The hero class id.</param>
    /// <param name="Name">The hero name.</param>
    /// <param name="Level">The resolve level.</param>
    /// <param name="ResolveLevel">The resolve experience level.</param>
    /// <param name="Cost">The recruitment cost, or <c>null</c> when not captured.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateHeroRecruited(
        string HeroId,
        string Class,
        string Name,
        int Level,
        int ResolveLevel,
        IReadOnlyList<EstateResourceAmount>? Cost = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
