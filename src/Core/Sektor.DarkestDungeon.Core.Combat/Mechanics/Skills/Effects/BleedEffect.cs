using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies a bleeding damage-over-time effect.</summary>
    public class BleedEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Bleeding; } }

        private int DotBleed { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BleedEffect"/> class.</summary>
        /// <param name="dotAmount">The damage per tick.</param>
        public BleedEffect(int dotAmount)
        {
            DotBleed = dotAmount;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            float bleedChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            bleedChance -= target.Character.GetSingleAttribute(AttributeType.Bleed).ModifiedValue;
            if (performer != null && !performer.Character.IsMonster)
                bleedChance += performer.Character.GetSingleAttribute(AttributeType.BleedChance).ModifiedValue;

            bleedChance = ChanceMath.Clamp01(bleedChance);
            if (RandomSolver.CheckSuccess(bleedChance))
            {
                var bleedStatus = (IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Bleeding);
                bleedStatus.AddInstanse(DotBleed, effect.IntegerParams[EffectIntParams.Duration] ?? BattleConstants.DefaultDotDuration);
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
                battleContext.Events.ShowPopup(target, PopupType.Bleed);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.BleedResist);
                return false;
            }
        }
    }
}

