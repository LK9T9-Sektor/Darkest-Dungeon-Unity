namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Character;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

    [TestFixture]
    public class StressTests
    {
        [Test]
        public void Crit_AppliesStressToTheTargetHero()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var attacker = duel.HeroParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.CritChance)).RawValue = 1.0f;

            var skill = attacker.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.GetAvailableTargets(attacker, s).Count > 0);
            Assert.That(skill, Is.Not.Null, "The acting hero should have a usable damage skill.");

            var target = duel.GetAvailableTargets(attacker, skill)[0];
            int stressBefore = (int)target.Character.Stress.CurrentValue;

            duel.ExecuteSkill(attacker, target, skill);

            Assert.That((int)target.Character.Stress.CurrentValue, Is.EqualTo(stressBefore + 15),
                "A crit should stress the target hero by 15 (Effects['Stress 2']).");
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