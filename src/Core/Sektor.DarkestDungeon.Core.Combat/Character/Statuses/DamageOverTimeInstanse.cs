namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>A single damage-over-time instance (bleed/poison tick).</summary>
    public class DamageOverTimeInstanse
    {
        /// <summary>Gets or sets the damage per tick.</summary>
        public int TickDamage { get; set; }

        /// <summary>Gets or sets the remaining ticks.</summary>
        public int TicksLeft { get; set; }

        /// <summary>Gets or sets the total ticks amount.</summary>
        public int TicksAmount { get; set; }

        /// <summary>Decrements the remaining ticks.</summary>
        /// <returns>True if the instance expired.</returns>
        public bool CheckExpiration()
        {
            return --TicksLeft <= 0;
        }
    }
}