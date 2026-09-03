using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies a poison damage-over-time effect.</summary>
    public class PoisonEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Poison; } }

        private int DotPoison { get; set; }

        /// <summary>Gets the damage per tick.</summary>
        public int DotAmount { get { return DotPoison; } }

        /// <summary>Initializes a new instance of the <see cref="PoisonEffect"/> class.</summary>
        /// <param name="dotAmount">The damage per tick.</param>
        public PoisonEffect(int dotAmount)
        {
            DotPoison = dotAmount;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            float poisonChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            poisonChance -= target.Character.GetSingleAttribute(AttributeType.Poison).ModifiedValue;
            if (performer != null && !performer.Character.IsMonster)
                poisonChance += performer.Character.GetSingleAttribute(AttributeType.PoisonChance).ModifiedValue;

            poisonChance = ChanceMath.Clamp01(poisonChance);
            if (RandomSolver.CheckSuccess(poisonChance))
            {
                var poisonStatus = (IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Poison);
                poisonStatus.AddInstanse(DotPoison, effect.IntegerParams[EffectIntParams.Duration] ?? BattleConstants.DefaultDotDuration);
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
                int duration = effect.IntegerParams[EffectIntParams.Duration] ?? BattleConstants.DefaultDotDuration;
                battleContext.Events.ShowPopup(target, PopupType.Poison, DotPoison + " dmg x " + duration + " rounds");
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.PoisonResist);
                return false;
            }
        }
    }
}

