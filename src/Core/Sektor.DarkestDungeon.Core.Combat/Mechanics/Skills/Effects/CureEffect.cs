using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Cures bleed and poison from a target.</summary>
    public class CureEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Cure; } }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            ((IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Poison)).RemoveDoT();
            ((IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Bleeding)).RemoveDoT();
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            bool cureEffective = false;
            var poisonStatus = (IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Poison);
            var bleedStatus = (IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Bleeding);
            if (poisonStatus.IsApplied)
            {
                poisonStatus.RemoveDoT();
                cureEffective = true;
            }
            if (bleedStatus.IsApplied)
            {
                bleedStatus.RemoveDoT();
                cureEffective = true;
            }
            if (cureEffective)
            {
                battleContext.Events.ShowPopup(target, PopupType.Cured);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            return false;
        }
    }
}