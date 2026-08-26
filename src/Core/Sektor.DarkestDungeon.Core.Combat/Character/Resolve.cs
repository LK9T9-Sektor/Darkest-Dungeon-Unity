namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Minimal hero resolve level holder.</summary>
    public class Resolve
    {
        /// <summary>Gets or sets the current experience.</summary>
        public int CurrentXP { get; set; }

        /// <summary>Gets or sets the resolve level.</summary>
        public int Level { get; set; }

        /// <summary>Gets or sets the experience required for the next level.</summary>
        public int NextLevelXP { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Resolve"/> class.</summary>
        /// <param name="level">The resolve level.</param>
        /// <param name="currentXP">The current experience.</param>
        public Resolve(int level, int currentXP)
        {
            Level = level;
            CurrentXP = currentXP;
        }
    }
}