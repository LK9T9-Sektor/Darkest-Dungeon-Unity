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
    public sealed record CombatantState(
        string Id,
        string Name,
        int Hp,
        int MaxHp,
        bool IsEnemy,
        string Icon,
        string Skeleton);
}
