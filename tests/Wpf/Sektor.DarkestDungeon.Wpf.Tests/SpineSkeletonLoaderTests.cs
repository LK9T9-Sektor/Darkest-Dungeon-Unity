using System;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Spine.Wpf;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Loads the bundled Crusader idle skeleton through the Spine 2.1 runtime.</summary>
    [TestFixture]
    public class SpineSkeletonLoaderTests
    {
        /// <summary>The loader must parse the shipped 2.1.18 binary skeleton with its atlas texture.</summary>
        [Test]
        public void CrusaderIdle_LoadsWithExpectedAnimation()
        {
            using var content = LoadCrusaderIdle();

            Assert.That(content.SkeletonData.Version, Is.EqualTo("2.1.18"));
            Assert.That(content.SkeletonData.FindAnimation("idle"), Is.Not.Null);
            Assert.That(content.SkeletonData.DefaultSkin, Is.Not.Null);
            Assert.That(content.DefaultAnimationName, Is.EqualTo("idle"));
        }

        /// <summary>Playing a known animation succeeds; an unknown name is rejected.</summary>
        [Test]
        public void Player_PlaysIdle_AndRejectsUnknownAnimation()
        {
            using var content = LoadCrusaderIdle();
            using var player = content.CreatePlayer();

            Assert.DoesNotThrow(() => player.Play("idle"));
            Assert.That(player.CurrentAnimation, Is.EqualTo("idle"));
            Assert.Throws<ArgumentException>(() => player.Play("no-such-animation"));
        }

        internal static SpineContent LoadCrusaderIdle()
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "Content", "Spine", "Crusader", "Idle");
            return new SpineSkeletonLoader().Load(
                Path.Combine(directory, "crusader.sprite.idle.skel"),
                Path.Combine(directory, "crusader.sprite.idle.atlas"));
        }
    }
}
