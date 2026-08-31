using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    [TestFixture]
    public class HeroGenerationTests
    {
        private static HeroClass BuildClass(string stringId)
        {
            var heroClass = new HeroClass { StringId = stringId };
            heroClass.Attributes[AttributeType.HitPoints] = 40;
            heroClass.Attributes[AttributeType.SpeedRating] = 0;
            heroClass.Attributes[AttributeType.AttackRating] = 0;
            heroClass.Attributes[AttributeType.CritChance] = 0;
            heroClass.Attributes[AttributeType.DamageLow] = 5;
            heroClass.Attributes[AttributeType.DamageHigh] = 8;
            heroClass.Attributes[AttributeType.DefenseRating] = 0;
            heroClass.Attributes[AttributeType.ProtectionRating] = 0;
            return heroClass;
        }

        [Test]
        public void GenerateHero_KnownClass_UsesCanonicalName()
        {
            Assert.That(HeroGeneration.GenerateHero(BuildClass("crusader"), 1).Name, Is.EqualTo("Reynauld"));
            Assert.That(HeroGeneration.GenerateHero(BuildClass("plague_doctor"), 1).Name, Is.EqualTo("Paracelsus"));
            Assert.That(HeroGeneration.GenerateHero(BuildClass("highwayman"), 1).Name, Is.EqualTo("Dismas"));
        }

        [Test]
        public void GenerateHero_KnownClass_NameDoesNotDependOnSeed()
        {
            Assert.That(
                HeroGeneration.GenerateHero(BuildClass("occultist"), 1).Name,
                Is.EqualTo(HeroGeneration.GenerateHero(BuildClass("occultist"), 999).Name));
        }

        [Test]
        public void GenerateHero_CanonicalNames_AreDistinctAcrossClasses()
        {
            string[] classIds =
            {
                "plague_doctor", "highwayman", "crusader", "vestal", "occultist",
                "man_at_arms", "hellion", "leper", "bounty_hunter", "grave_robber",
                "jester", "houndmaster", "abomination", "arbalest", "antiquarian",
            };

            var names = classIds.Select(classId => HeroGeneration.GenerateHero(BuildClass(classId), 7).Name).ToList();
            CollectionAssert.AllItemsAreUnique(names);
        }

        [Test]
        public void GenerateHero_UnknownClass_FallsBackToSeededDeterministicName()
        {
            var first = HeroGeneration.GenerateHero(BuildClass("custom_class"), 42);
            var second = HeroGeneration.GenerateHero(BuildClass("custom_class"), 42);

            Assert.That(second.Name, Is.EqualTo(first.Name));
            Assert.That(first.Name, Is.Not.Empty);
        }
    }
}