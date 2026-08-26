namespace Sektor.DarkestDungeon.Core.Combat.Battle
{
    /// <summary>Cooldown tracker for a combat skill.</summary>
    public class SkillCooldown
    {
        /// <summary>Gets the skill identifier.</summary>
        public string SkillId { get; }

        /// <summary>Gets the remaining cooldown amount.</summary>
        public int Amount { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="SkillCooldown"/> class.</summary>
        /// <param name="skillId">The skill identifier.</param>
        /// <param name="amount">The cooldown amount.</param>
        public SkillCooldown(string skillId, int amount)
        {
            SkillId = skillId;
            Amount = amount;
        }

        /// <summary>Reduces the cooldown by one turn.</summary>
        /// <returns>True if the cooldown has expired.</returns>
        public bool ReduceCooldown()
        {
            return --Amount <= 0;
        }

        /// <summary>Creates a copy of this cooldown.</summary>
        /// <returns>A new <see cref="SkillCooldown"/> with the same values.</returns>
        public SkillCooldown Copy()
        {
            return new SkillCooldown(SkillId, Amount);
        }
    }
}
