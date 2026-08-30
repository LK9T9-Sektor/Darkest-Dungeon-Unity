using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Content
{
    /// <summary>
    /// Named identifiers of content buffs referenced by id from <c>Data/JsonBuffs.json</c>.
    /// Using constants keeps buff lookups refactor-safe and avoids magic strings.
    /// </summary>
    public static class BuffIds
    {
        /// <summary>Stun recovery buff: +40% stun resistance for 2 rounds, applied when a stun wears off.</summary>
        public const string StunRecovery = "STUNRECOVERYBUFF";

        /// <summary>Creates the death's door survival debuff (-10% death blow resistance, 3 combat rounds).</summary>
        /// <returns>The survival debuff buff.</returns>
        public static Buff DeathsDoorSurvivalDebuff()
        {
            return new Buff(BuffType.StatAdd, AttributeType.DeathBlow, BattleConstants.DeathsDoorSurvivalValue)
            {
                Id = "",
                DurationAmount = BattleConstants.DeathsDoorSurvivalDuration,
                DurationType = BuffDurationType.Combat,
            };
        }
    }
}