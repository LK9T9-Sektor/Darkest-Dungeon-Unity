using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Content;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Applies the stun recovery buff when a stun wears off at the start of the stunned unit's turn.
    /// </summary>
    public class StunRecoveryApplier
    {
        private readonly IDuelContent content;

        /// <summary>Initializes a new instance of the <see cref="StunRecoveryApplier"/> class.</summary>
        /// <param name="content">The content source resolving the recovery buff.</param>
        public StunRecoveryApplier(IDuelContent content)
        {
            this.content = content;
        }

        /// <summary>Applies the STUNRECOVERYBUFF (+40% stun resistance, 2 rounds) to the unit.</summary>
        /// <param name="unit">The unit whose stun wore off.</param>
        public void Apply(ICombatUnit unit)
        {
            var recoveryBuff = content.GetBuff(BuffIds.StunRecovery);
            if (recoveryBuff == null)
                return;
            unit.Character.AddBuff(new BuffInfo(recoveryBuff, BuffDurationType.Round, BuffSourceType.Adventure, 2));
        }
    }
}