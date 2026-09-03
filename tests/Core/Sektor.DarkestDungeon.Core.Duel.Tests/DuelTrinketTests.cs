using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    /// <summary>Tests trinket equip in the duel: buffs affect stats, ids are recorded, unknown ids are ignored.</summary>
    [TestFixture]
    public class DuelTrinketTests
    {
        /// <summary>Equipping a trinket applies its permanent buff (accuracy stone raises accuracy).</summary>
        [Test]
        public void EquippingTrinket_AppliesItsPermanentBuffToTheHero()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(
                PicksWithTrinkets("crusader", "accuracy_stone"),
                Picks("highwayman"),
                42,
                isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0].Character as Hero;
            Assert.That(hero, Is.Not.Null);
            Assert.That(hero.EquippedTrinketIds, Does.Contain("accuracy_stone"));

            float accuracy = hero.GetSingleAttribute(AttributeType.AttackRating).ModifiedValue;
            Assert.That(accuracy, Is.EqualTo(0.04f).Within(0.0001f),
                "The accuracy stone should raise the attack rating by its permanent buff.");
        }

        /// <summary>An unknown trinket id is ignored without breaking the duel setup.</summary>
        [Test]
        public void UnknownTrinketId_IsIgnored()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(
                PicksWithTrinkets("crusader", "missing_trinket"),
                Picks("highwayman"),
                42,
                isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0].Character as Hero;
            Assert.That(hero, Is.Not.Null);
            Assert.That(hero.EquippedTrinketIds, Is.Empty);
        }

        /// <summary>Trinket ids are wired through the pick into the hero's equipped list.</summary>
        [Test]
        public void TrinketIds_AreRecordedOnTheHero()
        {
            var duel = new DuelController(new TestDuelContent());
            duel.StartDuel(
                PicksWithTrinkets("crusader", "accuracy_stone", "lucky_dice"),
                Picks("highwayman"),
                7,
                isHost: true);
            RandomSolver.SetRandomSeed(7);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0].Character as Hero;
            Assert.That(hero, Is.Not.Null);
            Assert.That(hero.EquippedTrinketIds, Has.Count.EqualTo(2));
            Assert.That(hero.EquippedTrinketIds, Contains.Item("accuracy_stone"));
            Assert.That(hero.EquippedTrinketIds, Contains.Item("lucky_dice"));
        }

        private static DuelHeroPick[] Picks(string classId)
        {
            return new[]
            {
                new DuelHeroPick(classId, 1),
                new DuelHeroPick(classId, 2),
                new DuelHeroPick(classId, 3),
                new DuelHeroPick(classId, 4),
            };
        }

        private static DuelHeroPick[] PicksWithTrinkets(string classId, params string[] trinketIds)
        {
            return new[]
            {
                new DuelHeroPick(classId, 1, null, null, trinketIds),
                new DuelHeroPick(classId, 2),
                new DuelHeroPick(classId, 3),
                new DuelHeroPick(classId, 4),
            };
        }
    }
}