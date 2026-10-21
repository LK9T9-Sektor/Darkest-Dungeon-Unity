using System.Collections.Generic;
using System.IO;
using System.Threading;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the skill catalog that joins parsed hero skills with the bundled art.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroSkillCatalogTests
    {
        /// <summary>Every bundled hero exposes its base-rank skills with an icon and both animation ids.</summary>
        [Test]
        public void GetSkills_EveryHero_HasIconsAndAnimationIds()
        {
            var catalog = CreateCatalog();

            foreach (string classId in DuelClasses.AllClassIds)
            {
                var skills = catalog.GetSkills(classId);
                int withEffect = 0;

                Assert.That(skills, Is.Not.Empty, classId);
                foreach (var skill in skills)
                {
                    Assert.That(skill.Icon, Is.Not.Null, $"{classId}/{skill.Skill.Id} icon");
                    Assert.That(skill.BaseAnimationId, Is.Not.Null.And.Not.Empty, $"{classId}/{skill.Skill.Id} base animation");
                    if (skill.EffectAnimationId != null)
                        withEffect++;
                }

                Assert.That(withEffect, Is.GreaterThan(0), classId + " has no effect animation id");
            }
        }

        /// <summary>The icon lookup by skill id matches the icon attached to the skill list.</summary>
        [Test]
        public void GetSkillIcon_MatchesSkillListIcon()
        {
            var catalog = CreateCatalog();
            var skill = catalog.GetSkills("crusader")[0];

            Assert.That(catalog.GetSkillIcon("crusader", skill.Skill.Id), Is.SameAs(skill.Icon));
        }

        /// <summary>An unknown class or skill yields no skills / no icon instead of throwing.</summary>
        [Test]
        public void UnknownClassOrSkill_ReturnsEmptyOrNull()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetSkills("unknown_hero"), Is.Empty);
            Assert.That(catalog.GetSkillIcon("crusader", "unknown_skill"), Is.Null);
            Assert.That(catalog.GetSkillIcon("unknown_hero", "smite"), Is.Null);
        }

        /// <summary>A class without an art entry still lists its skills, without icons or animation ids.</summary>
        [Test]
        public void GetSkills_WithoutArtEntry_HasNullIconAndAnimations()
        {
            var heroClass = new HeroClass { StringId = "test_hero" };
            heroClass.CombatSkills.Add(new CombatSkill { Id = "bare" });
            var catalog = new HeroSkillCatalog(new[] { heroClass }, new HeroIconCatalog(ContentRoot()));

            var skills = catalog.GetSkills("test_hero");

            Assert.That(skills, Has.Count.EqualTo(1));
            Assert.That(skills[0].Icon, Is.Null);
            Assert.That(skills[0].EffectAnimationId, Is.Null);
            Assert.That(skills[0].BaseAnimationId, Is.Null);
        }

        private static HeroSkillCatalog CreateCatalog()
            => new HeroSkillCatalog(DuelClasses.AllClasses, new HeroIconCatalog(ContentRoot()));

        private static string ContentRoot() =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "HeroIcons");
    }
}
