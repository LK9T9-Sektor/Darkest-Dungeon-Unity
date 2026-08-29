using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    internal sealed class RecordingSubEffect : SubEffect
    {
        public int InstantCalls { get; private set; }
        public int QueuedCalls { get; private set; }

        public override EffectSubType Type { get { return EffectSubType.Heal; } }

        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            InstantCalls++;
            return true;
        }

        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            QueuedCalls++;
            return true;
        }
    }
}