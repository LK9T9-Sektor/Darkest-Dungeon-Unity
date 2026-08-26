using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies the stun status to a target.</summary>
    public class StunEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Stun; } }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var stunStatus = (IStunStatusEffect)target.Character.GetStatusEffect(StatusType.Stun);
            if (stunStatus.IsApplied)
                return true;

            float stunChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            stunChance -= target.Character.GetSingleAttribute(AttributeType.Stun).ModifiedValue;
            if (performer != null && !performer.Character.IsMonster)
                stunChance += performer.Character.GetSingleAttribute(AttributeType.StunChance).ModifiedValue;

            stunChance = Clamp01(stunChance, 0.95f);
            if (RandomSolver.CheckSuccess(stunChance))
            {
                stunStatus.StunApplied = true;
                battleContext.Events.SetHalo(target, "stunned");
                ((IResetableStatusEffect)target.Character.GetStatusEffect(StatusType.Guard)).ResetStatus();
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
                battleContext.Events.ShowPopup(target, PopupType.Stunned);
                return true;
            }
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.StunResist);
                return false;
            }
        }

        private static float Clamp01(float value, float max)
        {
            if (value < 0)
                return 0;
            if (value > max)
                return max;
            return value;
        }
    }
}