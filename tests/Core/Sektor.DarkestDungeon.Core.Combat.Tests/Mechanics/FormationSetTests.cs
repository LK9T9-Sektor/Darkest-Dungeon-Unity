using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    [TestFixture]
    public class FormationSetTests
    {
        [Test]
        public void Parse_EmptyString_IsSelfTarget()
        {
            var set = new FormationSet("");

            Assert.That(set.IsSelfTarget, Is.True);
            Assert.That(set.IsSelfFormation, Is.True);
        }

        [Test]
        public void Parse_Ranks_AreSorted()
        {
            var set = new FormationSet("321");

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, set.Ranks);
        }

        [Test]
        public void Parse_Prefixes_FlagsAreSet()
        {
            var set = new FormationSet("@~?12");

            Assert.That(set.IsSelfFormation, Is.True);
            Assert.That(set.IsMultitarget, Is.True);
            Assert.That(set.IsRandomTarget, Is.True);
        }

        [Test]
        public void Parse_NoPrefix_TargetsEnemy()
        {
            var set = new FormationSet("1234");

            Assert.That(set.SkillTargetType, Is.EqualTo(SkillTargetType.Enemy));
        }

        [Test]
        public void Parse_SelfFormation_TargetsParty()
        {
            var set = new FormationSet("@1234");

            Assert.That(set.SkillTargetType, Is.EqualTo(SkillTargetType.Party));
        }

        [Test]
        public void IsLaunchableFrom_WithinRank_ReturnsTrue()
        {
            var set = new FormationSet("12");

            Assert.That(set.IsLaunchableFrom(1, 1), Is.True);
            Assert.That(set.IsLaunchableFrom(3, 1), Is.False);
        }

        [Test]
        public void IsLaunchableFrom_LargeUnit_SpansMultipleRanks()
        {
            var set = new FormationSet("12");

            Assert.That(set.IsLaunchableFrom(1, 2), Is.True);
        }

        [Test]
        public void IsTargetableUnit_RankAndSize_MatchOriginal()
        {
            var set = new FormationSet("1234");

            Assert.That(set.IsTargetableUnit(4, 1), Is.True);
            Assert.That(set.IsTargetableUnit(3, 2), Is.True);
        }
    }
}