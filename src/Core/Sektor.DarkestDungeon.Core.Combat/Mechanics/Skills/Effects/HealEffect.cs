using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Heals a target for a fixed amount, scaling with the performer's heal modifiers.</summary>
    public class HealEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Heal; } }

        private int HealAmount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HealEffect"/> class.</summary>
        /// <param name="amount">The base heal amount.</param>
        public HealEffect(int amount)
        {
            HealAmount = amount;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            float initialHeal = HealAmount;
            if (performer != null)
                initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

            if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
            {
                int critHeal = target.Character.Heal(initialHeal * 1.5f, true);
                battleContext.Events.UpdateOverlay(target);
                battleContext.Events.ShowPopup(target, PopupType.CritHeal, critHeal.ToString());
            }
            else
            {
                int heal = target.Character.Heal(initialHeal, true);
                battleContext.Events.UpdateOverlay(target);
                battleContext.Events.ShowPopup(target, PopupType.Heal, heal.ToString());
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            float initialHeal = HealAmount;
            if (performer != null)
                initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

            if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
            {
                int critHeal = target.Character.Heal(initialHeal * 1.5f, true);
                battleContext.Events.ShowPopup(target, PopupType.CritHeal, critHeal.ToString());
                if (target.Character.IsMonster)
                    battleContext.Events.PlaySound("event:/general/status/heal_enemy_crit");
                else
                    battleContext.Events.PlaySound("event:/general/status/heal_ally_crit");

                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            else
            {
                int heal = target.Character.Heal(initialHeal, true);
                battleContext.Events.ShowPopup(target, PopupType.Heal, heal.ToString());
                if (target.Character.IsMonster)
                    battleContext.Events.PlaySound("event:/general/status/heal_enemy");
                else
                    battleContext.Events.PlaySound("event:/general/status/heal_ally");

                battleContext.Events.UpdateOverlay(target);
                return true;
            }
        }
    }
}