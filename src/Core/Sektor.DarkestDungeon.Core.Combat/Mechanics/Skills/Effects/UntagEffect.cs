using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Removes the mark from a target.</summary>
    public class UntagEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Untag; } }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var markStatus = (IMarkStatusEffect)target.Character.GetStatusEffect(StatusType.Marked);
            if (markStatus.IsApplied)
            {
                markStatus.MarkDuration = 0;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (ApplyInstant(performer, target, effect, battleContext))
            {
                battleContext.Events.ShowPopup(target, PopupType.Untagged);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            return false;
        }
    }
}