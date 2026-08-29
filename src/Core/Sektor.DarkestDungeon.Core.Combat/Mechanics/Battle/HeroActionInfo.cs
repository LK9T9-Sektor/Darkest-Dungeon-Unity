namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Preview data for a hero action.</summary>
    public class HeroActionInfo
    {
        /// <summary>Gets or sets a value indicating whether the action is valid.</summary>
        public bool IsValid { get; private set; }

        /// <summary>Gets or sets the chance to hit.</summary>
        public float ChanceToHit { get; private set; }

        /// <summary>Gets or sets the chance to crit.</summary>
        public float ChanceToCrit { get; private set; }

        /// <summary>Gets or sets the minimum damage.</summary>
        public int MinDamage { get; private set; }

        /// <summary>Gets or sets the maximum damage.</summary>
        public int MaxDamage { get; private set; }

        /// <summary>Updates the action info with new values.</summary>
        /// <param name="valid">Whether the action is valid.</param>
        /// <param name="hit">Chance to hit.</param>
        /// <param name="crit">Chance to crit.</param>
        /// <param name="minDamage">Minimum damage.</param>
        /// <param name="maxDamage">Maximum damage.</param>
        public void UpdateInfo(bool valid, float hit, float crit, int minDamage, int maxDamage)
        {
            IsValid = valid;
            ChanceToCrit = crit;
            ChanceToHit = hit;
            MinDamage = minDamage;
            MaxDamage = maxDamage;
        }
    }
}
