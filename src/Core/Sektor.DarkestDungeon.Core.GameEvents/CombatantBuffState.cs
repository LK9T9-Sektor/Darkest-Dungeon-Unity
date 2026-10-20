namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One active buff in a combatant snapshot.</summary>
    /// <param name="Id">The buff content id (may be empty for generated buffs).</param>
    /// <param name="Attribute">The modified attribute name.</param>
    /// <param name="ModifierValue">The modifier value applied by the buff.</param>
    /// <param name="Duration">Remaining duration in turns, or <c>0</c> for permanent buffs.</param>
    /// <param name="DurationType">The duration category (for example <c>Combat</c> or <c>Camp</c>).</param>
    /// <param name="Source">The buff source category.</param>
    /// <param name="IsApplied">Whether the buff is currently applied to the stats.</param>
    public sealed record CombatantBuffState(
        string Id,
        string Attribute,
        float ModifierValue,
        int Duration = 0,
        string? DurationType = null,
        string? Source = null,
        bool IsApplied = false);
}
