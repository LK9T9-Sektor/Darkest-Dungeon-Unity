namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

    [TestFixture]
    public class ModeTests
    {
        [Test]
        public void ModeHero_StartsInHumanMode_AndTransformSwitchesToBeast()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("abomination"), Picks("crusader"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[1];
            Assert.That(hero.Character.CurrentMode, Is.Not.Null, "Mode heroes start in their raid-default mode.");
            Assert.That(hero.Character.CurrentMode.Id, Is.EqualTo("human"));

            var manacles = hero.Character.CurrentCombatSkills.Single(skill => skill.Id == "manacles");
            var rage = hero.Character.CurrentCombatSkills.Single(skill => skill.Id == "rage");
            var transform = hero.Character.CurrentCombatSkills.Single(skill => skill.Id == "transform");
            Assert.That(transform.Category, Is.EqualTo(SkillCategory.Support));

            Assert.That(duel.IsSkillUsable(hero, manacles), Is.True, "Manacles work in human mode.");
            Assert.That(duel.IsSkillUsable(hero, rage), Is.False, "Rage is a beast-mode skill.");

            var target = duel.GetAvailableTargets(hero, transform).Single();
            duel.ExecuteSkill(hero, target, transform);

            Assert.That(hero.Character.CurrentMode.Id, Is.EqualTo("beast"),
                "Transform should switch the hero to beast mode.");
            Assert.That(duel.IsSkillUsable(hero, rage), Is.True, "After transforming, rage becomes usable.");
            Assert.That(duel.IsSkillUsable(hero, manacles), Is.False, "Manacles are human-mode only.");
        }

        [Test]
        public void ContinueTurnSkill_GrantsAnExtraAction()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("abomination"), Picks("crusader"), 4, isHost: true);
            RandomSolver.SetRandomSeed(4);
            duel.StartBattle();

            Assert.That(duel.IsLocalTurn, Is.True, "The fast abomination should take the first turn.");

            var hero = duel.CurrentUnit;
            Assert.That(hero.Rank, Is.LessThanOrEqualTo(3), "The acting hero must be able to transform.");
            Assert.That(hero.Team, Is.EqualTo(Sektor.DarkestDungeon.Core.Combat.Raid.Battle.Team.Heroes));

            var transform = hero.Character.CurrentCombatSkills.Single(skill => skill.Id == "transform");
            Assert.That(duel.IsSkillUsable(hero, transform), Is.True);

            string result = duel.ExecuteLocalSkill("transform", hero.CombatInfo.CombatId);

            Assert.That(result, Is.Not.Null, "transform should execute via ExecuteLocalSkill.");
            Assert.That(duel.CurrentUnit, Is.EqualTo(hero),
                "The transform is a continue-turn skill: the same hero should act again.");
            Assert.That(duel.Phase, Is.EqualTo(DuelPhase.WaitingForHostAction));
            Assert.That(hero.Character.CurrentMode.Id, Is.EqualTo("beast"));
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