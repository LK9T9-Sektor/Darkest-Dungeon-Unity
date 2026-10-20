using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// A unit as recorded in snapshots and stat events. Assets use logical names, never file paths.
    /// </summary>
    /// <param name="Id">The instance id (e.g. <c>crusader_01</c>).</param>
    /// <param name="Name">The display/class name.</param>
    /// <param name="Hp">Current hit points.</param>
    /// <param name="MaxHp">Maximum hit points.</param>
    /// <param name="IsEnemy">Whether the unit fights on the monster side.</param>
    /// <param name="Icon">Logical icon id.</param>
    /// <param name="Skeleton">Logical spine skeleton id.</param>
    /// <param name="Rank">Formation rank (1..4), or <c>0</c> for recordings made before schema 1.1.</param>
    /// <param name="Size">Formation size (1..4), or <c>1</c> for recordings made before schema 1.1.</param>
    /// <param name="Stress">Current stress.</param>
    /// <param name="MaxStress">Maximum stress, or <c>0</c> when not applicable.</param>
    /// <param name="AtDeathsDoor">Whether the unit is at Death's Door.</param>
    /// <param name="IsDead">Whether the unit is dead or a corpse.</param>
    /// <param name="IsAfflicted">Whether the unit is afflicted.</param>
    /// <param name="IsVirtued">Whether the unit is virtued.</param>
    /// <param name="Statuses">Active status effects, or <c>null</c> when not captured.</param>
    /// <param name="Buffs">Active buffs, or <c>null</c> when not captured.</param>
    public sealed record CombatantState(
        string Id,
        string Name,
        int Hp,
        int MaxHp,
        bool IsEnemy,
        string Icon,
        string Skeleton,
        int Rank = 0,
        int Size = 1,
        int Stress = 0,
        int MaxStress = 0,
        bool AtDeathsDoor = false,
        bool IsDead = false,
        bool IsAfflicted = false,
        bool IsVirtued = false,
        IReadOnlyList<CombatantStatusState>? Statuses = null,
        IReadOnlyList<CombatantBuffState>? Buffs = null);
}
