namespace Sektor.DarkestDungeon.Core.Combat.Content
{
    /// <summary>
    /// Named identifiers of combat effects referenced by id from the Effects catalog
    /// (<c>Data/Mechanics/Effects.txt</c>). Using constants keeps content lookups
    /// refactor-safe and avoids magic strings in battle code.
    /// </summary>
    public static class EffectIds
    {
        /// <summary>Stress applied on a crit ("Stress 2", 15 stress).</summary>
        public const string Stress2 = "Stress 2";

        /// <summary>Stress applied to the party by an afflicted hero.</summary>
        public const string AfflictedAllyStress = "AfflictedAllyStress";

        /// <summary>Stress healed on a crit heal.</summary>
        public const string CritHealStressHeal = "crit_heal_stress_heal";

        /// <summary>Stress healed on a kill (heal stress chance).</summary>
        public const string HealStressChance1 = "Heal Stress Chance 1";

        /// <summary>Stress healed when an enemy dies.</summary>
        public const string HealStress1 = "Heal Stress 1";
    }
}