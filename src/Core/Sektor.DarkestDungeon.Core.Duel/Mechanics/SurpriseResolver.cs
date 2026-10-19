using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Resolves the first-round surprise at battle start: which side is surprised depends on the
    /// monster battle modifiers, hero surprise chances and the torch level. A surprised side acts
    /// last in the first round and its heroes are shuffled.
    /// </summary>
    public class SurpriseResolver
    {
        private readonly FormationParty heroParty;
        private readonly FormationParty monsterParty;
        private readonly BattleGround battleGround;
        private readonly int torchAmount;

        /// <summary>Initializes a new instance of the <see cref="SurpriseResolver"/> class.</summary>
        /// <param name="heroParty">The hero party.</param>
        /// <param name="monsterParty">The monster party.</param>
        /// <param name="battleGround">The battlefield (for setting the surprise status).</param>
        /// <param name="torchAmount">The current torch amount.</param>
        public SurpriseResolver(FormationParty heroParty, FormationParty monsterParty, BattleGround battleGround, int torchAmount)
        {
            this.heroParty = heroParty;
            this.monsterParty = monsterParty;
            this.battleGround = battleGround;
            this.torchAmount = torchAmount;
        }

        /// <summary>Rolls surprise and applies the surprised status (plus hero shuffle).</summary>
        public void Resolve()
        {
            bool monsterSideModded = monsterParty.Units.Any(unit => unit.Character.BattleModifiers != null);

            if (monsterSideModded && monsterParty.Units.Any(unit => unit.Character.BattleModifiers.AlwaysBeSurprised))
            {
                SurpriseMonsters();
                return;
            }

            if (!monsterSideModded || monsterParty.Units.Any(unit => unit.Character.BattleModifiers.CanBeSurprised))
            {
                float monstersSurprised = BattleConstants.BaseSurpriseChance + TorchSurpriseBonus(torchAmount, true);
                foreach (var hero in heroParty.Units)
                {
                    var attribute = hero.Character.GetSingleAttribute(AttributeType.MonsterSurpirseChance);
                    if (attribute != null)
                        monstersSurprised += attribute.ModifiedValue;
                }
                monstersSurprised = ClampSurpriseChance(monstersSurprised);

                if (RandomSolver.CheckSuccess(monstersSurprised))
                {
                    SurpriseMonsters();
                    return;
                }
            }

            if (monsterSideModded && monsterParty.Units.Any(unit => unit.Character.BattleModifiers.AlwaysSurprise))
            {
                SurpriseHeroes();
                return;
            }

            if (!monsterSideModded || monsterParty.Units.Any(unit => unit.Character.BattleModifiers.CanSurprise))
            {
                float heroesSurprised = BattleConstants.BaseSurpriseChance + TorchSurpriseBonus(torchAmount, false);
                foreach (var hero in heroParty.Units)
                {
                    var attribute = hero.Character.GetSingleAttribute(AttributeType.PartySurpriseChance);
                    if (attribute != null)
                        heroesSurprised += attribute.ModifiedValue;
                }
                heroesSurprised = ClampSurpriseChance(heroesSurprised);

                if (RandomSolver.CheckSuccess(heroesSurprised))
                    SurpriseHeroes();
            }
        }

        private void SurpriseMonsters()
        {
            battleGround.SetSurpriseStatus(SurpriseStatus.MonstersSurprised);
            foreach (var unit in monsterParty.Units)
                unit.CombatInfo.IsSurprised = true;
        }

        private void SurpriseHeroes()
        {
            battleGround.SetSurpriseStatus(SurpriseStatus.HeroesSurprised);
            foreach (var unit in heroParty.Units)
                unit.CombatInfo.IsSurprised = true;
            ShuffleParty(heroParty);
        }

        private static float ClampSurpriseChance(float chance)
        {
            if (chance < 0f)
                return 0f;
            if (chance > BattleConstants.MaxSurpriseChance)
                return BattleConstants.MaxSurpriseChance;
            return chance;
        }

        private static float TorchSurpriseBonus(int torch, bool monsters)
        {
            if (torch > 75)
                return monsters ? 0.25f : 0f;
            if (torch > 50)
                return monsters ? 0.15f : 0f;
            if (torch > 25)
                return monsters ? 0.10f : 0.15f;
            if (torch > 0)
                return monsters ? 0.05f : 0.25f;
            return monsters ? 0f : 0.4f;
        }

        private static void ShuffleParty(FormationParty party)
        {
            for (int i = 0; i < party.Units.Count; i++)
            {
                int swapIndex = RandomSolver.Next(party.Units.Count);
                if (swapIndex == i)
                    continue;

                var temp = party.Units[i];
                party.Units[i] = party.Units[swapIndex];
                party.Units[swapIndex] = temp;
            }

            party.RecalculateRanks();
        }
    }
}