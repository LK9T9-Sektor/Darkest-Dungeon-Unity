using System;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the hero icon files bundled into <c>Content\HeroIcons</c> and their loading.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroIconCatalogTests
    {
        private static readonly string[] ClassIds =
        {
            "abomination", "antiquarian", "arbalest", "bounty_hunter", "crusader",
            "grave_robber", "hellion", "highwayman", "houndmaster", "jester",
            "leper", "man_at_arms", "occultist", "plague_doctor", "vestal",
        };

        private static readonly string[] Outfits = { "A", "B", "C", "D" };

        /// <summary>Every bundled hero resolves a roster portrait for all four outfits.</summary>
        [Test]
        public void GetPortrait_EveryHeroAndOutfit_IsBundled()
        {
            var catalog = CreateCatalog();

            foreach (string classId in ClassIds)
            {
                foreach (string outfit in Outfits)
                    Assert.That(catalog.GetPortrait(classId, outfit), Is.Not.Null, $"{classId} outfit {outfit}");
            }
        }

        /// <summary>Every bundled hero ships the wide guild header art next to the portraits.</summary>
        [Test]
        public void GuildHeader_EveryHero_IsBundled()
        {
            string root = ContentRoot();

            foreach (string classId in ClassIds)
                Assert.That(File.Exists(Path.Combine(root, classId, "header.png")), Is.True, $"{classId} header");
        }

        /// <summary>Every bundled hero loads its guild header art for the editor overlay.</summary>
        [Test]
        public void GetHeader_EveryHero_Loads()
        {
            var catalog = CreateCatalog();

            foreach (string classId in ClassIds)
                Assert.That(catalog.GetHeader(classId), Is.Not.Null, classId);
        }

        /// <summary>An unknown hero yields null instead of throwing.</summary>
        [Test]
        public void GetHeader_UnknownHero_ReturnsNull()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetHeader("unknown_hero"), Is.Null);
            Assert.That(catalog.GetHeader(string.Empty), Is.Null);
        }

        /// <summary>An unknown hero or outfit yields null instead of throwing.</summary>
        [Test]
        public void GetPortrait_UnknownHeroOrOutfit_ReturnsNull()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetPortrait("unknown_hero", "A"), Is.Null);
            Assert.That(catalog.GetPortrait("crusader", "Z"), Is.Null);
            Assert.That(catalog.GetPortrait(string.Empty, "A"), Is.Null);
        }

        /// <summary>The outfit letter is case insensitive.</summary>
        [Test]
        public void GetPortrait_LowerCaseOutfit_Resolves()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetPortrait("crusader", "b"), Is.Not.Null);
        }

        /// <summary>A portrait is read once and reused by later calls.</summary>
        [Test]
        public void GetPortrait_SecondCall_ReturnsCachedInstance()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetPortrait("crusader", "A"), Is.SameAs(catalog.GetPortrait("crusader", "A")));
        }

        /// <summary>Every bundled hero ships its seven ability icons and they load.</summary>
        [Test]
        public void GetAbilityIcon_EveryHeroHasSevenIcons()
        {
            string[] icons = { "one", "two", "three", "four", "five", "six", "seven" };
            var catalog = CreateCatalog();

            foreach (string classId in ClassIds)
            {
                foreach (string iconId in icons)
                    Assert.That(catalog.GetAbilityIcon(classId, iconId), Is.Not.Null, $"{classId} ability {iconId}");
            }
        }

        /// <summary>The ability icons keep their native 72x72 art size.</summary>
        [Test]
        public void GetAbilityIcon_IsNative72Square()
        {
            var image = (BitmapSource)CreateCatalog().GetAbilityIcon("crusader", "one")!;

            Assert.That(image.PixelWidth, Is.EqualTo(72));
            Assert.That(image.PixelHeight, Is.EqualTo(72));
        }

        /// <summary>An unknown hero or icon id yields null instead of throwing.</summary>
        [Test]
        public void GetAbilityIcon_UnknownHeroOrIcon_ReturnsNull()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetAbilityIcon("unknown_hero", "one"), Is.Null);
            Assert.That(catalog.GetAbilityIcon("crusader", "zero"), Is.Null);
            Assert.That(catalog.GetAbilityIcon(string.Empty, "one"), Is.Null);
        }

        /// <summary>An ability icon is read once and reused by later calls.</summary>
        [Test]
        public void GetAbilityIcon_SecondCall_ReturnsCachedInstance()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.GetAbilityIcon("crusader", "one"), Is.SameAs(catalog.GetAbilityIcon("crusader", "one")));
        }

        private static HeroIconCatalog CreateCatalog() => new HeroIconCatalog(ContentRoot());

        private static string ContentRoot() =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "HeroIcons");
    }
}
