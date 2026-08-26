using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Sets the hero's current mode (e.g. religious or alternation).</summary>
    public class SetModeEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Mode; } }

        private string Mode { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SetModeEffect"/> class.</summary>
        /// <param name="mode">The mode identifier.</param>
        public SetModeEffect(string mode)
        {
            Mode = mode;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            battleContext.Events.SetCombatAnimation(target, false);

            target.Character.CurrentMode = target.Character.Modes.Find(mode => mode.Id == Mode);

            battleContext.Events.SetCombatAnimation(target, true);

            battleContext.Events.UpdateSkillPanel(target);

            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}