using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Takes control of a target for a duration (monster captor skill).</summary>
    public class ControlEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Control; } }

        private int Duration { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ControlEffect"/> class.</summary>
        /// <param name="duration">The control duration.</param>
        public ControlEffect(int duration)
        {
            Duration = duration;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || !performer.Character.IsMonster)
                return false;

            if (performer.Character.ControllerCaptor == null)
                return false;

            float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
            debuffChance = performer == target ? 1 : Clamp01(debuffChance, 0.95f);

            if (RandomSolver.CheckSuccess(debuffChance))
            {
                battleContext.Events.ControlUnit(target, performer, Duration);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (ApplyInstant(performer, target, effect, battleContext))
                return true;

            battleContext.Events.ShowPopup(target, PopupType.DebuffResist);
            return false;
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