using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Removes the stun status from a target.</summary>
    public class UnstunEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Unstun; } }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var stunStatus = (IStunStatusEffect)target.Character.GetStatusEffect(StatusType.Stun);
            if (stunStatus.IsApplied)
            {
                stunStatus.StunApplied = false;
                battleContext.Events.ResetHalo(target);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (ApplyInstant(performer, target, effect, battleContext))
            {
                battleContext.Events.ShowPopup(target, PopupType.Unstun);
                return true;
            }
            return false;
        }
    }
}