namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Heal component for skills that restore health.</summary>
    public class HealComponent
    {
        /// <summary>Gets the minimum heal amount.</summary>
        public int MinAmount { get; }

        /// <summary>Gets the maximum heal amount.</summary>
        public int MaxAmount { get; }

        /// <summary>Initializes a new instance of the <see cref="HealComponent"/> class.</summary>
        /// <param name="min">Minimum heal amount.</param>
        /// <param name="max">Maximum heal amount.</param>
        public HealComponent(int min, int max)
        {
            MinAmount = min;
            MaxAmount = max;
        }
    }
}
