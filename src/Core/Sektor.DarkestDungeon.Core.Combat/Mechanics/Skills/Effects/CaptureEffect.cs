using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Captures a target into an empty captor monster.</summary>
    public class CaptureEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Capture; } }

        private bool RemoveFromParty { get; set; }
        private bool VirtueBlockable { get; set; }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (target.Character.IsVirtued && VirtueBlockable)
                return false;

            var emptyCaptorUnit = performer.Party.Units.Find(unit => unit.Character.IsMonster
                && unit.Character.EmptyCaptor != null
                && unit.Character.EmptyCaptor.PerformerBaseClass == performer.Character.Class);

            if (emptyCaptorUnit == null)
                return false;

            string fullMonsterTypeId = emptyCaptorUnit.Character.EmptyCaptor.FullMonsterClass;
            ICombatUnit fullCaptorUnit = battleContext.Events.ReplaceUnit(fullMonsterTypeId, emptyCaptorUnit);

            battleContext.Events.CaptureUnit(target, fullCaptorUnit, RemoveFromParty);

            if (RemoveFromParty == false)
            {
                battleContext.Events.ApplyCaptorEffects(emptyCaptorUnit, target);
                battleContext.Events.SetCaptureEffect(target, fullCaptorUnit);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (ApplyInstant(performer, target, effect, battleContext))
                return true;

            if (target.Character.IsVirtued && VirtueBlockable)
                battleContext.Events.ShowPopup(target, PopupType.DebuffResist);
            return false;
        }
    }
}