using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Summons monsters into the performer's formation.</summary>
    public class SummonMonstersEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Summon; } }

        /// <summary>Gets or sets the number of monsters to summon.</summary>
        public int SummonCount { get; set; }

        /// <summary>Gets or sets a value indicating whether summoned monsters can drop loot.</summary>
        public bool CanSpawnLoot { get; set; }

        /// <summary>Gets the summonable monster type ids.</summary>
        public List<string> SummonMonsters { get; }

        /// <summary>Gets the summon chances.</summary>
        public List<float> SummonChances { get; }

        /// <summary>Gets the summon limits per monster.</summary>
        public List<int> SummonLimits { get; }

        /// <summary>Gets the summon rank formations.</summary>
        public List<FormationSet> SummonRanks { get; }

        /// <summary>Gets the summon roll initiatives.</summary>
        public List<int> SummonRollInitiatives { get; }

        /// <summary>Initializes a new instance of the <see cref="SummonMonstersEffect"/> class.</summary>
        public SummonMonstersEffect()
        {
            SummonMonsters = new List<string>();
            SummonChances = new List<float>();
            SummonLimits = new List<int>();
            SummonRanks = new List<FormationSet>();
            SummonRollInitiatives = new List<int>();
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
                if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                    return false;

            var summonPool = new List<int>();
            var chancePool = new List<float>(SummonChances);
            for (int i = 0; i < SummonMonsters.Count; i++)
                summonPool.Add(i);

            for (int i = 0; i < SummonCount; i++)
            {
                if (summonPool.Count == 0)
                    break;

                int rolledIndex = RandomSolver.ChooseRandomIndex(chancePool);
                int summonIndex = summonPool[rolledIndex];
                if (SummonLimits.Count > 0)
                {
                    if (SummonLimits[summonIndex] <= performer.Party.Units.FindAll(unit =>
                        unit.Character.Name == SummonMonsters[summonIndex]).Count)
                    {
                        i--;
                        summonPool.RemoveAt(rolledIndex);
                        chancePool.RemoveAt(rolledIndex);
                        continue;
                    }
                }
                if (battleContext.Events.AvailableSummonSpace < battleContext.Events.GetMonsterSize(SummonMonsters[summonIndex]))
                {
                    i--;
                    summonPool.RemoveAt(rolledIndex);
                    chancePool.RemoveAt(rolledIndex);
                    continue;
                }
                bool rollInitiative = SummonRollInitiatives.Count > 0;
                if (SummonRanks.Count > 0)
                    battleContext.Events.SummonUnit(SummonMonsters[summonIndex],
                        SummonRanks[summonIndex].Ranks[RandomSolver.Next(SummonRanks[summonIndex].Ranks.Count)],
                        rollInitiative, CanSpawnLoot);
                else
                    battleContext.Events.SummonUnit(SummonMonsters[summonIndex], 1, rollInitiative, CanSpawnLoot);
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}