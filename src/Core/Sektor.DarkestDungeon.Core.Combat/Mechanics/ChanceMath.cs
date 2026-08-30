using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics
{
    /// <summary>
    /// Shared chance math used by effect resolution: clamping a 0..1 chance to a maximum.
    /// </summary>
    public static class ChanceMath
    {
        /// <summary>Clamps a chance to the [0, <paramref name="max"/>] range.</summary>
        /// <param name="value">The raw chance.</param>
        /// <param name="max">The maximum allowed chance (defaults to the battle cap 0.95).</param>
        /// <returns>The clamped chance.</returns>
        public static float Clamp01(float value, float max = BattleConstants.MaxChance)
        {
            if (value < 0f)
                return 0f;
            if (value > max)
                return max;
            return value;
        }
    }
}