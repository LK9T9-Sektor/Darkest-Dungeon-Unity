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

            poisonChance = Clamp01(poisonChance, 0.95f);
            if (RandomSolver.CheckSuccess(poisonChance))
            {
                var poisonStatus = (IDotStatusEffect)target.Character.GetStatusEffect(StatusType.Poison);
                poisonStatus.AddInstanse(DotPoison, effect.IntegerParams[EffectIntParams.Duration] ?? 3);
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
                battleContext.Events.ShowPopup(target, PopupType.Poison);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.PoisonResist);
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