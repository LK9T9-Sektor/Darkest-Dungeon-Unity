using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the bundled camping-skill catalog used by the hero editor.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CampingSkillCatalogTests
    {
        /// <summary>A class returns only its own camping skills; all icons are delivered.</summary>
        [Test]
        public void GetForClass_ReturnsClassSkillsAndIcons()
        {
            var skills = CampingSkillCatalog.GetForClass("crusader");

            Assert.That(skills, Is.Not.Empty);
            Assert.That(skills.All(skill => skill.HeroClasses.Any(
                classId => classId.Equals("crusader", StringComparison.OrdinalIgnoreCase))), Is.True);
            Assert.That(CampingSkillCatalog.GetIcon(skills[0].Id), Is.Not.Null);
        }

        /// <summary>Unknown and empty class ids return an empty list instead of throwing.</summary>
        [TestCase("")]
        [TestCase("not_a_hero")]
        public void GetForClass_UnknownClass_ReturnsEmpty(string classId)
        {
            Assert.That(CampingSkillCatalog.GetForClass(classId), Is.Empty);
        }
    }
}
