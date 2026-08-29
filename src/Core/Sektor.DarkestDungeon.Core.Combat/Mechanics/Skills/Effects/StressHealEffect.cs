using System;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Heals stress of a hero with fuse support.</summary>
    public class StressHealEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.StressHeal; } }

        /// <inheritdoc/>
        public override bool Fusable { get { return true; } }

        private int StressHealAmount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="StressHealEffect"/> class.</summary>
        /// <param name="amount">The base stress heal amount.</param>
        public StressHealEffect(int amount)
        {
            StressHealAmount = amount;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            float initialHeal = StressHealAmount;
            if (performer != null)
                initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

            int heal = RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
            if (heal < 1) heal = 1;

            target.Character.Stress.DecreaseValue(heal);
            if (RoundToInt(target.Character.Stress.CurrentValue) == 0 && target.Character.IsAfflicted)
                target.Character.RevertTrait();
            battleContext.Events.UpdateOverlay(target);
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            float initialHeal = StressHealAmount;
            if (performer != null)
                initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

            int heal = RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
            if (heal < 1) heal = 1;

            target.Character.Stress.DecreaseValue(heal);
            if (RoundToInt(target.Character.Stress.CurrentValue) == 0 && target.Character.IsAfflicted)
                target.Character.RevertTrait();
            battleContext.Events.UpdateOverlay(target);
            battleContext.Events.ShowPopup(target, PopupType.StressHeal, heal.ToString());
            battleContext.Events.SetHalo(target, "heroic");
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyFused(ICombatUnit performer, ICombatUnit target, Effect effect, int fuseParameter, IBattleContext battleContext)
        {
            if (target == null || fuseParameter <= 0 || target.Character.IsMonster)
                return false;

            target.Character.Stress.DecreaseValue(fuseParameter);
            if (RoundToInt(target.Character.Stress.CurrentValue) == 0 && target.Character.IsAfflicted)
                target.Character.RevertTrait();
            battleContext.Events.UpdateOverlay(target);
            battleContext.Events.ShowPopup(target, PopupType.StressHeal, fuseParameter.ToString());
            battleContext.Events.SetHalo(target, "heroic");
            return true;
        }

        /// <inheritdoc/>
        public override int Fuse(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return 0;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return 0;

            float initialHeal = StressHealAmount;
            if (performer != null)
                initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

            int heal = RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
            if (heal < 1) heal = 1;

            return heal;
        }

        private static int RoundToInt(float value)
        {
            return (int)Math.Round(value);
        }
    }
}