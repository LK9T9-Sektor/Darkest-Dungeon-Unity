using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One hero in the estate roster.</summary>
    /// <param name="Id">The stable hero id (<c>hero_&lt;rosterId&gt;</c>).</param>
    /// <param name="Class">The hero class id.</param>
    /// <param name="Name">The generated hero name.</param>
    /// <param name="Level">The resolve level.</param>
    /// <param name="ResolveLevel">The resolve experience level.</param>
    /// <param name="Hp">Current hit points.</param>
    /// <param name="MaxHp">Maximum hit points.</param>
    /// <param name="Stress">Current stress.</param>
    /// <param name="Status">The estate status (for example <c>Available</c> or <c>Tavern</c>).</param>
    /// <param name="IsAlive">Whether the hero is alive.</param>
    /// <param name="IsAfflicted">Whether the hero is afflicted.</param>
    /// <param name="IsVirtued">Whether the hero is virtued.</param>
    /// <param name="Quirks">Active quirk ids, or <c>null</c> when not captured.</param>
    /// <param name="Diseases">Active disease ids, or <c>null</c> when not captured.</param>
    public sealed record EstateHeroState(
        string Id,
        string Class,
        string Name,
        int Level,
        int ResolveLevel,
        int Hp,
        int MaxHp,
        int Stress,
        string Status,
        bool IsAlive,
        bool IsAfflicted,
        bool IsVirtued,
        IReadOnlyList<string>? Quirks = null,
        IReadOnlyList<string>? Diseases = null);
}
