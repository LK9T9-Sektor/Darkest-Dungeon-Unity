using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Shuffles the positions of a party or a single target.</summary>
    public class ShuffleTargetEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Shuffle; } }

        private bool IsPartyShuffle { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ShuffleTargetEffect"/> class.</summary>
        /// <param name="isPartyShuffle">Whether the whole party is shuffled.</param>
        public ShuffleTargetEffect(bool isPartyShuffle)
        {
            IsPartyShuffle = isPartyShuffle;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Party.Units.Count < 2)
                return false;

            if (IsPartyShuffle)
            {
                var shuffleUnits = new List<ICombatUnit>(target.Party.Units);
                foreach (var unit in shuffleUnits)
                {
                    var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                    var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                    if (shuffleRoll.Rank < unit.Rank)
                        battleContext.Events.Pull(unit, unit.Rank - shuffleRoll.Rank);
                    else
                        battleContext.Events.Push(unit, shuffleRoll.Rank - unit.Rank);
                }
                shuffleUnits.Clear();
                return true;
            }
            else
            {
                float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && !performer.Character.IsMonster)
                    moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                moveChance = Clamp01(moveChance, 0.95f);
                if (RandomSolver.CheckSuccess(moveChance))
                {
                    var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                    var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                    if (shuffleRoll.Rank < target.Rank)
                        battleContext.Events.Pull(target, target.Rank - shuffleRoll.Rank);
                    else
                        battleContext.Events.Push(target, shuffleRoll.Rank - target.Rank);
                    return true;
                }
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Party.Units.Count < 2)
                return false;

            if (IsPartyShuffle)
            {
                foreach (var unit in target.Party.Units)
                {
                    float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    moveChance -= unit.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                    moveChance = Clamp01(moveChance, 0.95f);
                    if (RandomSolver.CheckSuccess(moveChance))
                    {
                        var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                        var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                        if (shuffleRoll.Rank < unit.Rank)
                            battleContext.Events.Pull(unit, unit.Rank - shuffleRoll.Rank);
                        else
                            battleContext.Events.Push(unit, shuffleRoll.Rank - unit.Rank);
                        return true;
                    }
                }
                return true;
            }
            else
            {
                float moveChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                moveChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && !performer.Character.IsMonster)
                    moveChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                moveChance = Clamp01(moveChance, 0.95f);
                if (RandomSolver.CheckSuccess(moveChance))
                {
                    var shuffleTargets = target.Party.Units.FindAll(unit => unit != target);
                    var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                    if (shuffleRoll.Rank < target.Rank)
                        battleContext.Events.Pull(target, target.Rank - shuffleRoll.Rank);
                    else
                        battleContext.Events.Push(target, shuffleRoll.Rank - target.Rank);
                    return true;
                }
                battleContext.Events.ShowPopup(target, PopupType.MoveResist);
                return false;
            }
        }

        private static float Clamp01(float value, float max)
        {
            if (value < 0)
                return 0;
            if (value > max)
                return max;
            return value;
        }
    }
}