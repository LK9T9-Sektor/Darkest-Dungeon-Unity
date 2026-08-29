using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
    [TestFixture]
    public class HeroSkillSelectionTests
    {
        private static HeroClass BuildClass()
        {
            return new HeroClass
            {
                Attributes = new Dictionary<AttributeType, float> { { AttributeType.HitPoints, 30f } },
                Resistances = new Dictionary<AttributeType, float>(),
                Tags = new List<string>(),
                CombatSkills = new List<CombatSkill>
                {
                    new CombatSkill { Id = "a" },
                    new CombatSkill { Id = "b" },
                    new CombatSkill { Id = "c" },
                },
            };
        }

        [Test]
        public void SelectCombatSkills_RestrictsCurrentCombatSkills()
        {
            var hero = new Hero(BuildClass(), 0, "Test");
            hero.SelectCombatSkills(new[] { "a", "c" });

            CollectionAssert.AreEquivalent(new[] { "a", "c" }, hero.CurrentCombatSkills.Select(s => s.Id));
        }

        [Test]
        public void SelectCombatSkills_UnknownIdsAreIgnored()
        {
            var hero = new Hero(BuildClass(), 0, "Test");
            hero.SelectCombatSkills(new[] { "a", "missing" });

            CollectionAssert.AreEquivalent(new[] { "a" }, hero.CurrentCombatSkills.Select(s => s.Id));
        }

        [Test]
        public void EmptySelection_FallsBackToAllSkills()
        {
            var hero = new Hero(BuildClass(), 0, "Test");

            CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, hero.CurrentCombatSkills.Select(s => s.Id));
        }

        [Test]
        public void AddQuirk_RecordsUniqueIds()
        {
            var hero = new Hero(BuildClass(), 0, "Test");

            hero.AddQuirk("tough");
            hero.AddQuirk("tough");

            CollectionAssert.Contains(hero.Quirks, "tough");
            Assert.That(hero.Quirks.Count, Is.EqualTo(1));
        }
    }
}