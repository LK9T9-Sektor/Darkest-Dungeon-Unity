using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the hero folder name to class id conversion.</summary>
    [TestFixture]
    public class HeroIdsTests
    {
        /// <summary>Every spine hero folder maps to the snake_case class id of its icon folder.</summary>
        [TestCase("Abomination", "abomination")]
        [TestCase("Antiquarian", "antiquarian")]
        [TestCase("Arbalest", "arbalest")]
        [TestCase("BountyHunter", "bounty_hunter")]
        [TestCase("Crusader", "crusader")]
        [TestCase("GraveRobber", "grave_robber")]
        [TestCase("Hellion", "hellion")]
        [TestCase("Highwayman", "highwayman")]
        [TestCase("Houndmaster", "houndmaster")]
        [TestCase("Jester", "jester")]
        [TestCase("Leper", "leper")]
        [TestCase("ManAtArms", "man_at_arms")]
        [TestCase("Occultist", "occultist")]
        [TestCase("PlagueDoctor", "plague_doctor")]
        [TestCase("Vestal", "vestal")]
        public void ToClassId_SpineHeroFolder_ReturnsSnakeCaseId(string heroName, string expected)
        {
            Assert.That(HeroIds.ToClassId(heroName), Is.EqualTo(expected));
        }

        /// <summary>An already snake_case id is left unchanged.</summary>
        [Test]
        public void ToClassId_SnakeCaseId_IsUnchanged()
        {
            Assert.That(HeroIds.ToClassId("man_at_arms"), Is.EqualTo("man_at_arms"));
        }

        /// <summary>An empty name yields an empty id.</summary>
        [Test]
        public void ToClassId_EmptyName_ReturnsEmpty()
        {
            Assert.That(HeroIds.ToClassId(string.Empty), Is.EqualTo(string.Empty));
        }
    }
}
