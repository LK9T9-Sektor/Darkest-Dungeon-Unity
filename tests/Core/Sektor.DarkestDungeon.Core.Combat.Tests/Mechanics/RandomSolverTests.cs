namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;

    [TestFixture]
    public class RandomSolverTests
    {
        [TearDown]
        public void ResetSeed()
        {
            RandomSolver.SetRandomSeed(0);
        }

        [Test]
        public void SetRandomSeed_ProducesReproducibleSequence()
        {
            RandomSolver.SetRandomSeed(12345);
            int firstRun = RandomSolver.Next(1000);

            RandomSolver.SetRandomSeed(12345);
            int secondRun = RandomSolver.Next(1000);

            Assert.That(firstRun, Is.EqualTo(secondRun));
        }

        [Test]
        public void Next_MaxValue_IsWithinBounds()
        {
            RandomSolver.SetRandomSeed(7);
            for (int i = 0; i < 100; i++)
            {
                int value = RandomSolver.Next(10);
                Assert.That(value, Is.InRange(0, 9));
            }
        }

        [Test]
        public void CheckSuccess_AlwaysTrueAtOne()
        {
            Assert.That(RandomSolver.CheckSuccess(1f), Is.True);
        }

        [Test]
        public void CheckSuccess_AlwaysFalseAtZero()
        {
            Assert.That(RandomSolver.CheckSuccess(0f), Is.False);
        }

        [Test]
        public void CheckSuccess_WithHighChance_MostlyTrue()
        {
            RandomSolver.SetRandomSeed(42);
            int successes = 0;
            for (int i = 0; i < 1000; i++)
                if (RandomSolver.CheckSuccess(0.99f))
                    successes++;

            Assert.That(successes, Is.GreaterThan(950));
        }
    }
}