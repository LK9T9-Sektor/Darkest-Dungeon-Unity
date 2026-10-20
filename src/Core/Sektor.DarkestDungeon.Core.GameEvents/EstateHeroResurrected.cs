using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A dead hero was resurrected in the estate.</summary>
    /// <param name="HeroId">The resurrected hero id.</param>
    /// <param name="Class">The hero class id.</param>
    /// <param name="Name">The hero name.</param>
    /// <param name="Cost">The resurrection cost, or <c>null</c> when not captured.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateHeroResurrected(
        string HeroId,
        string Class,
        string Name,
        IReadOnlyList<EstateResourceAmount>? Cost = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
