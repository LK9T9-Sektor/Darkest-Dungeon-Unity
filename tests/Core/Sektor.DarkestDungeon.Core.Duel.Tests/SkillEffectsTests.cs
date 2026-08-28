namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Character;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics;

    [TestFixture]
    public class SkillEffectsTests
    {
        [Test]
        public void StatBuffSkill_AppliesStatBuffsToThePerformer()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("highwayman"), Picks("crusader"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[2];
            var takeAim = hero.Character.CurrentCombatSkills.FirstOrDefault(skill => skill.Id == "take_aim");
            Assert.That(takeAim, Is.Not.Null, "The highwayman should know take_aim.");

            var targets = duel.GetAvailableTargets(hero, takeAim);
            Assert.That(targets.Count, Is.GreaterThan(0), "take_aim should have a valid target from rank 3.");

            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            float attackBefore = ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.AttackRating)).ModifiedValue;
            float damageLowBefore = ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DamageLow)).ModifiedValue;

            duel.ExecuteSkill(hero, targets[0], takeAim);

            float attackAfter = ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.AttackRating)).ModifiedValue;
            float damageLowAfter = ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DamageLow)).ModifiedValue;

            Assert.That(attackAfter, Is.EqualTo(attackBefore + 0.06f).Within(0.001f),
                "take_aim should add 6% attack rating (Highwayman Buff 1).");
            Assert.That(damageLowAfter, Is.EqualTo(damageLowBefore * 1.12f).Within(0.001f),
                "take_aim should multiply minimum damage by 1.12 (Highwayman Buff 1).");
        }

        [Test]
        public void StunSkill_AppliesTheStunStatusToTheTarget()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var stunningBlow = hero.Character.CurrentCombatSkills.FirstOrDefault(skill => skill.Id == "stunning_blow");
            Assert.That(stunningBlow, Is.Not.Null, "The crusader should know stunning_blow.");

            var target = duel.GetAvailableTargets(hero, stunningBlow).FirstOrDefault();
            Assert.That(target, Is.Not.Null, "stunning_blow should have a valid target from rank 1.");

            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            ((SingleAttribute)target.Character.GetSingleAttribute(AttributeType.Stun)).RawValue = 0f;

            duel.ExecuteSkill(hero, target, stunningBlow);

            Assert.That(target.Character.GetStatusEffect(StatusType.Stun).IsApplied, Is.True,
                "stunning_blow should stun the target (Stun 1).");
        }

        private static DuelHeroPick[] Picks(string classId)
        {
            return new[]
            {
                new DuelHeroPick(classId, 1),
                new DuelHeroPick(classId, 2),
                new DuelHeroPick(classId, 3),
                new DuelHeroPick(classId, 4),
            };
        }
    }
}