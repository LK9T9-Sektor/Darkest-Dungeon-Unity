namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a damage-over-time status effect (bleed/poison).</summary>
    public interface IDotStatusEffect : IStatusEffect
    {
        /// <summary>Adds a new damage-over-time instance.</summary>
        /// <param name="tickDamage">The damage per tick.</param>
        /// <param name="ticks">The number of ticks.</param>
        void AddInstanse(int tickDamage, int ticks);

        /// <summary>Removes all damage-over-time instances.</summary>
        void RemoveDoT();
    }
}