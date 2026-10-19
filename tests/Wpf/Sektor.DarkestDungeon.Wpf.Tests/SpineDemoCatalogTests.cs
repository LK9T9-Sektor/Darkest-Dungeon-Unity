using System;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the file system catalog over the linked spine demo content.</summary>
    [TestFixture]
    public class SpineDemoCatalogTests
    {
        /// <summary>All bundled hero folders are found.</summary>
        [Test]
        public void GetCharacters_ReturnsBundledHeroes()
        {
            var catalog = CreateCatalog();

            var characters = catalog.GetCharacters();

            Assert.That(characters, Has.Count.EqualTo(15));
            Assert.That(characters, Does.Contain("Crusader"));
            Assert.That(characters, Does.Contain("ManAtArms"));
            Assert.That(characters, Does.Contain("Vestal"));
        }

        /// <summary>The idle set is listed first, the other sets (base + Effects skill sets) follow alphabetically.</summary>
        [Test]
        public void GetAnimations_PutsIdleFirst()
        {
            var catalog = CreateCatalog();

            var animations = catalog.GetAnimations("Crusader");

            Assert.That(animations[0], Is.EqualTo("Idle"));
            Assert.That(animations, Does.Contain("Walk"));
            Assert.That(animations, Does.Contain("AttackSword"));
            Assert.That(animations, Does.Contain("Smite"));
            Assert.That(animations, Has.Count.EqualTo(23), "14 base sets + 9 Effects skill sets");
            Assert.That(catalog.GetAnimations("NoSuchHero"), Is.Empty);
        }

        /// <summary>The content directory points at the hero animation set folder.</summary>
        [Test]
        public void GetContentDirectory_ComposesHeroAnimationPath()
        {
            string root = Path.Combine(AppContext.BaseDirectory, "Content", "Spine");
            var catalog = new SpineDemoCatalog(root);

            string directory = catalog.GetContentDirectory("Crusader", "Idle");

            Assert.That(directory, Is.EqualTo(Path.Combine(root, "Crusader", "Idle")));
            Assert.That(File.Exists(Path.Combine(directory, "crusader.sprite.idle.skel")), Is.True);
        }

        private static SpineDemoCatalog CreateCatalog()
            => new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine"));
    }
}
