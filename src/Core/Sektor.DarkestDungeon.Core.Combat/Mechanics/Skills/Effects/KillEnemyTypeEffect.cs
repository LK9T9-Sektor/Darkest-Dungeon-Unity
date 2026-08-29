using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Kills a monster of a specific type (e.g. corpses).</summary>
    public class KillEnemyTypeEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.KillType; } }

        private MonsterType EnemyType { get; set; }

        /// <summary>Initializes a new instance of the <see cref="KillEnemyTypeEffect"/> class.</summary>
        /// <param name="monsterType">The monster type to kill.</param>
        public KillEnemyTypeEffect(MonsterType monsterType)
        {
            EnemyType = monsterType;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (target.Character.IsMonster && target.Character.MonsterTypes.Contains(EnemyType))
            {
                target.Character.TakeDamagePercent(1.0f);
                target.CombatInfo.MarkedForDeath = true;
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