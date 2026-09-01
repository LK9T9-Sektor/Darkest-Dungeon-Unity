using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Pulls a target forward in the formation.</summary>
    public class PullEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Pull; } }

        private int PullParam { get; set; }

        /// <summary>Gets the pull distance.</summary>
        public int Param { get { return PullParam; } }

        /// <summary>Initializes a new instance of the <see cref="PullEffect"/> class.</summary>
        /// <param name="pullParam">The pull distance.</param>
        public PullEffect(int pullParam)
        {
            PullParam = pullParam;
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
                battleContext.Events.Pull(target, PullParam);
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
