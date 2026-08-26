using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Immobilizes a target so it cannot move ranks.</summary>
    public class ImmobilizeEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Immobilize; } }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            ApplyInstant(performer, target, effect, battleContext);
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (!target.CombatInfo.IsImmobilized)
            {
                target.CombatInfo.IsImmobilized = true;
                battleContext.Events.SetDefendAnimation(target, true);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}