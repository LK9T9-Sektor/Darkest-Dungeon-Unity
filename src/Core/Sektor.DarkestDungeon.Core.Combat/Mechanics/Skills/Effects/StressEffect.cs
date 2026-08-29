using System;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Deals stress damage to a hero with fuse support.</summary>
    public class StressEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Stress; } }

        /// <inheritdoc/>
        public override bool Fusable { get { return true; } }

        private int StressAmount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="StressEffect"/> class.</summary>
        /// <param name="amount">The base stress damage.</param>
        public StressEffect(int amount)
        {
            StressAmount = amount;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            float initialDamage = StressAmount;
            if (performer != null)
                initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

            int damage = RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
            if (damage < 1) damage = 1;

            target.Character.Stress.IncreaseValue(damage);
            HandleOverstress(target, battleContext);
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

            float initialDamage = StressAmount;
            if (performer != null)
                initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

            int damage = RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
            if (damage < 1) damage = 1;

            target.Character.Stress.IncreaseValue(damage);
            HandleOverstress(target, battleContext);
            battleContext.Events.UpdateOverlay(target);
            battleContext.Events.ShowPopup(target, PopupType.Stress, damage.ToString());
            battleContext.Events.SetHalo(target, "afflicted");
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyFused(ICombatUnit performer, ICombatUnit target, Effect effect, int fuseParameter, IBattleContext battleContext)
        {
            if (target == null || fuseParameter <= 0 || target.Character.IsMonster)
                return false;

            target.Character.Stress.IncreaseValue(fuseParameter);
            HandleOverstress(target, battleContext);
            battleContext.Events.UpdateOverlay(target);
            battleContext.Events.ShowPopup(target, PopupType.Stress, fuseParameter.ToString());
            battleContext.Events.SetHalo(target, "afflicted");
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

            float initialDamage = StressAmount;
            if (performer != null)
                initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

            int damage = RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
            if (damage < 1) damage = 1;

            return damage;
        }

        private static void HandleOverstress(ICombatUnit target, IBattleContext battleContext)
        {
            if (!target.Character.IsOverstressed)
                return;

            if (target.Character.IsVirtued)
                target.Character.Stress.DecreaseValue((int)(target.Character.Stress.CurrentValue -
                    Clamp(target.Character.Stress.CurrentValue, 0, 100)));
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                battleContext.Events.AddResolveCheck(target);

            if (Approximately(target.Character.Stress.CurrentValue, 200))
                battleContext.Events.AddHeartAttackCheck(target);
        }

        private static int RoundToInt(float value)
        {
            return (int)Math.Round(value);
        }

        private static float Clamp(float value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 0.000001f;
        }
    }
}