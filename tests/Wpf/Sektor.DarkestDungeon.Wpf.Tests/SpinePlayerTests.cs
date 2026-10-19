using System;

using NUnit.Framework;

using Sektor.DarkestDungeon.Spine.Wpf;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the player's time scale forwarding to the Spine animation state.</summary>
    [TestFixture]
    public class SpinePlayerTests
    {
        /// <summary>The time scale defaults to normal speed and is settable.</summary>
        [Test]
        public void TimeScale_DefaultsToNormalSpeed()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var player = content.CreatePlayer();

            Assert.That(player.TimeScale, Is.EqualTo(1f));

            player.TimeScale = 4f;
            Assert.That(player.TimeScale, Is.EqualTo(4f));
        }

        /// <summary>Double time scale for half the time lands on the same pose as normal speed for the full time.</summary>
        [Test]
        public void TimeScale_DoublesAnimationProgress()
        {
            using var fastContent = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var slowContent = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var referenceContent = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var fastPlayer = fastContent.CreatePlayer();
            using var slowPlayer = slowContent.CreatePlayer();
            using var referencePlayer = referenceContent.CreatePlayer();

            fastPlayer.Play("idle");
            slowPlayer.Play("idle");
            referencePlayer.Play("idle");
            fastPlayer.TimeScale = 2f;
            fastPlayer.Update(0.15f);
            slowPlayer.Update(0.3f);
            referencePlayer.Update(0.15f);

            var fastBones = fastPlayer.Skeleton.Bones.Items;
            var slowBones = slowPlayer.Skeleton.Bones.Items;
            var referenceBones = referencePlayer.Skeleton.Bones.Items;
            bool anyDifferentFromReference = false;
            for (int i = 0; i < slowPlayer.Skeleton.Bones.Count; i++)
            {
                Assert.That(fastBones[i].WorldX, Is.EqualTo(slowBones[i].WorldX).Within(1e-4f));
                Assert.That(fastBones[i].WorldY, Is.EqualTo(slowBones[i].WorldY).Within(1e-4f));

                if (Math.Abs(fastBones[i].WorldX - referenceBones[i].WorldX) > 1e-4f
                    || Math.Abs(fastBones[i].WorldY - referenceBones[i].WorldY) > 1e-4f)
                    anyDifferentFromReference = true;
            }

            Assert.That(anyDifferentFromReference, Is.True);
        }
    }
}
