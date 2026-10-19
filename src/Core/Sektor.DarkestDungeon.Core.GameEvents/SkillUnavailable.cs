using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A skill could not be used - negative test case for consumers (spec §5).</summary>
    /// <param name="Skill">The skill id.</param>
    /// <param name="Reason">Why it was unavailable.</param>
    /// <param name="CooldownRemaining">Remaining cooldown, when the reason is a cooldown.</param>
    /// <param name="Input">The attempted cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record SkillUnavailable(
        string Skill,
        string Reason,
        int? CooldownRemaining = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
