namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// Unit data rendered by the shared battle field (<c>BattleFieldView</c>): what a
    /// <c>BattleUnitCardView</c> needs, independent of whether the battle is live or replayed.
    /// </summary>
    public interface IBattleFieldUnit
    {
        /// <summary>Gets the formation rank (1-based).</summary>
        int Rank { get; }

        /// <summary>Gets the card width in pixels for the occupied ranks.</summary>
        double CardWidth { get; }

        /// <summary>Gets the current hit points.</summary>
        int Hp { get; }

        /// <summary>Gets the maximum hit points.</summary>
        int MaxHp { get; }

        /// <summary>Gets the current stress.</summary>
        int Stress { get; }

        /// <summary>Gets the maximum stress.</summary>
        int MaxStress { get; }

        /// <summary>Gets the Spine content directory currently shown.</summary>
        string ContentDirectory { get; }

        /// <summary>Gets a value indicating whether the current animation loops.</summary>
        bool Loop { get; }

        /// <summary>Gets the skeleton id used by the fallback card.</summary>
        string Skeleton { get; }

        /// <summary>Gets a value indicating whether the unit belongs to the enemy side.</summary>
        bool IsEnemy { get; }

        /// <summary>Gets a value indicating whether the unit is the current attacker.</summary>
        bool IsAttacker { get; }

        /// <summary>Gets a value indicating whether the unit is highlighted as a target.</summary>
        bool IsTarget { get; }

        /// <summary>Gets a value indicating whether the unit is dead.</summary>
        bool IsDead { get; }
    }
}
