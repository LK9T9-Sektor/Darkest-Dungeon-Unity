namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;

    [TestFixture]
    public class MonsterBrainTests
    {
        [Test]
        public void Constructor_InitializesEmptyDesireSets()
        {
            var brain = new MonsterBrain();

            Assert.That(brain.SkillDesireSet, Is.Empty);
            Assert.That(brain.TargetDesireSet, Is.Empty);
            Assert.That(brain.BonusDesireSet, Is.Empty);
            Assert.That(brain.SkillCooldowns, Is.Empty);
        }

        [Test]
        public void Decision_Constructor_SetsTypeAndSelfTarget()
        {
            var decision = new MonsterBrainDecision(BrainDecisionType.Perform);

            Assert.That(decision.Decision, Is.EqualTo(BrainDecisionType.Perform));
            Assert.That(decision.TargetInfo.Type, Is.EqualTo(SkillTargetType.Self));
        }

        [Test]
        public void Decision_Default_IsPass()
        {
            var decision = new MonsterBrainDecision(BrainDecisionType.Pass);

            Assert.That(decision.SelectedSkill, Is.Null);
        }
    }
}