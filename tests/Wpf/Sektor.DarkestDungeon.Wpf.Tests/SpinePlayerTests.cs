using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.Spine.Skia;

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

        /// <summary>A one-shot play raises AnimationCompleted exactly once with the animation name.</summary>
        [Test]
        public void Play_WithoutLoop_RaisesAnimationCompletedOnce()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var player = content.CreatePlayer();
            var completed = new List<string>();
            player.AnimationCompleted += completed.Add;

            player.Play("idle", loop: false);
            for (int i = 0; i < 400 && completed.Count == 0; i++)
                player.Update(0.05f);

            Assert.That(completed, Has.Count.EqualTo(1));
            Assert.That(completed[0], Is.EqualTo("idle"));

            for (int i = 0; i < 40; i++)
                player.Update(0.05f);

            Assert.That(completed, Has.Count.EqualTo(1), "completion must not re-raise after the animation ended");
        }

        /// <summary>A looping play never raises AnimationCompleted, no matter how long it runs.</summary>
        [Test]
        public void Play_WithLoop_DoesNotRaiseAnimationCompleted()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var player = content.CreatePlayer();
            var completed = new List<string>();
            player.AnimationCompleted += completed.Add;

            player.Play("idle", loop: true);
            for (int i = 0; i < 400; i++)
                player.Update(0.05f);

            Assert.That(completed, Is.Empty);
        }

        /// <summary>Replaying with loop false after a cancelled one-shot still reports a single completion.</summary>
        [Test]
        public void Play_RestartsOneShotWithoutDuplicateCompletion()
        {
            using var content = SpineSkeletonLoaderTests.LoadCrusaderIdle();
            using var player = content.CreatePlayer();
            var completed = new List<string>();
            player.AnimationCompleted += completed.Add;

            player.Play("idle", loop: false);
            player.Update(0.05f);
            player.Play("idle", loop: false);
            for (int i = 0; i < 400 && completed.Count == 0; i++)
                player.Update(0.05f);

            Assert.That(completed, Has.Count.EqualTo(1));
        }
        /// <summary>
        /// The real AttackSword set parses; its animation is a zero-duration pose stub (measured:
        /// duration=0,4 bones), so the player's guard must still complete a one-shot play - this is
        /// the exact content the arena attack pose uses.
        /// </summary>
        [Test]
        public void AttackSwordSet_ZeroDurationPose_StillCompletesOneShot()
        {
            using var content = LoadCrusaderSet("AttackSword");
            Assert.That(content.DefaultAnimationName, Is.Not.Empty, "attack set must contain at least one animation");

            using var player = content.CreatePlayer();
            var completed = new List<string>();
            player.AnimationCompleted += completed.Add;

            player.Play(content.DefaultAnimationName, loop: false);
            for (int i = 0; i < 60 && completed.Count == 0; i++)
                player.Update(0.05f);

            Assert.That(completed, Has.Count.EqualTo(1));
            Assert.That(completed[0], Is.EqualTo("attack_sword"));
        }

        /// <summary>The arena's skill-effect overlay set (Smite, real1.867s animation) completes a one-shot play.</summary>
        [Test]
        public void SmiteSet_LoadsAndCompletesOneShot()
        {
            using var content = LoadCrusaderSet("Smite");
            Assert.That(content.DefaultAnimationName, Is.EqualTo("smite"));

            using var player = content.CreatePlayer();
            var completed = new List<string>();
            player.AnimationCompleted += completed.Add;

            float duration = content.SkeletonData.Animations.Items[0].Duration;
            Assert.That(duration, Is.GreaterThan(0f), "skill overlay must be a real timed animation");

            player.Play(content.DefaultAnimationName, loop: false);
            for (int i = 0; i < (duration / 0.05f) + 40 && completed.Count == 0; i++)
                player.Update(0.05f);

            Assert.That(completed, Has.Count.EqualTo(1));
        }

        private static SpineContent LoadCrusaderSet(string setName)
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "Content", "Spine", "Crusader", setName);
            Assert.That(Directory.Exists(directory), Is.True, $"content missing: {directory}");

            string[] skeletonFiles = Directory.GetFiles(directory, "*.skel*");
            string[] atlasFiles = Directory.GetFiles(directory, "*.atlas*");
            Assert.That(skeletonFiles, Has.Length.EqualTo(1));
            Assert.That(atlasFiles, Has.Length.EqualTo(1));

            return new SpineSkeletonLoader().Load(skeletonFiles[0], atlasFiles[0]);
        }
    }
}
