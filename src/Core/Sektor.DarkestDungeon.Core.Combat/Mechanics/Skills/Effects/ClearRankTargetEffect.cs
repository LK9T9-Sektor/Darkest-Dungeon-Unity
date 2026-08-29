using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Clears the marked ranks of the opposing formation.</summary>
    public class ClearRankTargetEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.ClearTargetRanks; } }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            ApplyInstant(performer, target, effect, battleContext);
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (performer == null)
                return false;

            battleContext.Events.ClearRankMarks(target);
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}