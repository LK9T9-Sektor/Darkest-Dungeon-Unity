using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Marks a target for increased damage.</summary>
    public class TagEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Tag; } }

        private DurationType DurationType { get; set; }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var markStatus = (IMarkStatusEffect)target.Character.GetStatusEffect(StatusType.Marked);
            markStatus.MarkDuration = effect.IntegerParams[EffectIntParams.Duration] ?? BattleConstants.DefaultMarkDuration;
            markStatus.DurationType = DurationType;
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (ApplyInstant(performer, target, effect, battleContext))
            {
                battleContext.Events.ShowPopup(target, PopupType.Tagged);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            return false;
        }
    }
}
