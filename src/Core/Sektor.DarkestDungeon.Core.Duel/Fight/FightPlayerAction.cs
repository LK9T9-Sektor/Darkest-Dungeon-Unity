namespace Sektor.DarkestDungeon.Core.Duel.Fight
{
    /// <summary>A manually issued action for the player-controlled side of a <see cref="FightSession"/>.</summary>
    public sealed class FightPlayerAction
    {
        /// <summary>Initializes a new instance of the <see cref="FightPlayerAction"/> class.</summary>
        /// <param name="skillId">The selected combat skill id.</param>
        /// <param name="targetCombatId">The selected target unit combat id.</param>
        public FightPlayerAction(string skillId, int targetCombatId)
        {
            SkillId = skillId;
            TargetCombatId = targetCombatId;
        }

        /// <summary>Gets the selected combat skill id.</summary>
        public string SkillId { get; private set; }

        /// <summary>Gets the selected target unit combat id.</summary>
        public int TargetCombatId { get; private set; }
    }
}