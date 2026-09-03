using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Pushes a target backward in the formation.</summary>
    public class PushEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Push; } }

        private int PushParam { get; set; }

        /// <summary>Gets the push distance.</summary>
        public int Param { get { return PushParam; } }

        /// <summary>Initializes a new instance of the <see cref="PushEffect"/> class.</summary>
        /// <param name="pushParam">The push distance.</param>
        public PushEffect(int pushParam)
        {
            PushParam = pushParam;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

            moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
            if (performer != null && !performer.Character.IsMonster)
                moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

            moveChance = ChanceMath.Clamp01(moveChance);
            if (RandomSolver.CheckSuccess(moveChance))
            {
                battleContext.Events.Push(target, PushParam);
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
                return true;
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.MoveResist);
                return false;
            }
        }
    }
}
