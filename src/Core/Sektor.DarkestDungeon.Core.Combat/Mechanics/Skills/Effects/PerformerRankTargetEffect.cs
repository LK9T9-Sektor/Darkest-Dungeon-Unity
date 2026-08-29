using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Marks the performer's rank for targeting.</summary>
    public class PerformerRankTargetEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Rank; } }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            ApplyInstant(performer, target, effect, battleContext);
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || performer == null)
                return false;

            battleContext.Events.MarkRank(target);
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}