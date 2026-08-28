namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    using System.Collections.Generic;

    using NSubstitute;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Character;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

    [TestFixture]
    public class BattleSolverTests
    {
        [TearDown]
        public void ResetSeed()
        {
            RandomSolver.SetRandomSeed(0);
        }

        private static IAttribute MakeAttribute(float value)
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(value);
            return attribute;
        }

        private static (BattleSolver Solver, ICombatUnit Performer, ICombatUnit Target, CombatSkill Skill) SetupDamage(
            bool heroPerformer = true)
        {
            var performerAttribute = MakeAttribute(0f);
            var targetAttribute = MakeAttribute(0f);

            var performerChar = Substitute.For<ICharacter>();
            performerChar.IsMonster.Returns(!heroPerformer);
            performerChar.Accuracy.Returns(1f);
            performerChar.Dodge.Returns(0f);
            performerChar.MinDamage.Returns(10f);
            performerChar.MaxDamage.Returns(10f);
            performerChar.DamageMod.Returns(0f);
            performerChar.Protection.Returns(0f);
            performerChar.Crit.Returns(0f);
            performerChar.GetSingleAttribute(Arg.Any<AttributeType>()).Returns(performerAttribute);

            var targetChar = Substitute.For<ICharacter>();
            targetChar.IsMonster.Returns(false);
            targetChar.Dodge.Returns(0f);
            targetChar.Protection.Returns(0f);
            targetChar.HasZeroHealth.Returns(false);
            targetChar.GetSingleAttribute(Arg.Any<AttributeType>()).Returns(targetAttribute);
            targetChar.TakeDamage(Arg.Any<float>()).Returns(x => (int)(float)x[0]);
            targetChar.Heal(Arg.Any<float>(), Arg.Any<bool>()).Returns(x => (int)(float)x[0]);

            var performer = Substitute.For<ICombatUnit>();
            performer.Team.Returns(Team.Heroes);
            performer.Rank.Returns(1);
            performer.Size.Returns(1);
            performer.Character.Returns(performerChar);
            var performerInfo = Substitute.For<IFormationUnitInfo>();
            performerInfo.IsImmobilized.Returns(false);
            performerInfo.SkillsUsedThisTurn.Returns(new List<string>());
            performerInfo.SkillsUsedInBattle.Returns(new List<string>());
            performer.CombatInfo.Returns(performerInfo);

            var target = Substitute.For<ICombatUnit>();
            target.Team.Returns(Team.Monsters);
            target.Rank.Returns(1);
            target.Size.Returns(1);
            target.Character.Returns(targetChar);
            var targetInfo = Substitute.For<IFormationUnitInfo>();
            targetInfo.SkillsUsedThisTurn.Returns(new List<string>());
            targetInfo.SkillsUsedInBattle.Returns(new List<string>());
            target.CombatInfo.Returns(targetInfo);

            var heroParty = Substitute.For<IFormationParty>();
            heroParty.Units.Returns(new List<ICombatUnit> { performer });
            var monsterParty = Substitute.For<IFormationParty>();
            monsterParty.Units.Returns(new List<ICombatUnit> { target });

            var battleGround = Substitute.For<IBattleGround>();
            battleGround.HeroParty.Returns(heroParty);
            battleGround.MonsterParty.Returns(monsterParty);

            var events = Substitute.For<IBattleEvents>();
            var battleContext = Substitute.For<IBattleContext>();
            battleContext.BattleGround.Returns(battleGround);
            battleContext.Events.Returns(events);

            var skill = new CombatSkill
            {
                Category = SkillCategory.Damage,
                Accuracy = 1f,
                DamageMin = 10f,
                DamageMax = 10f,
                DamageMod = 0f,
                CritMod = 0f,
                IsCritValid = false,
                CanMiss = null,
                LaunchRanks = new FormationSet("1234"),
                TargetRanks = new FormationSet("1234"),
            };

            var solver = new BattleSolver(battleContext);
            return (solver, performer, target, skill);
        }

        [Test]
        public void IsSkillUsable_HeroSkill_WithValidTarget_ReturnsTrue()
        {
            var (solver, performer, target, skill) = SetupDamage();

            Assert.That(solver.IsSkillUsable(performer, skill), Is.True);
        }

        [Test]
        public void ExecuteSkill_Damage_DealsExpectedDamage()
        {
            var (solver, performer, target, skill) = SetupDamage();
            RandomSolver.SetRandomSeed(42);

            solver.ExecuteSkill(performer, target, skill, null);

            Assert.That(solver.SkillResult.SkillEntries, Has.Count.EqualTo(1));
            Assert.That(solver.SkillResult.SkillEntries[0].Type, Is.EqualTo(SkillResultType.Hit));
            Assert.That(solver.SkillResult.SkillEntries[0].Amount, Is.EqualTo(10));
        }

        [Test]
        public void ExecuteSkill_Heal_AddsHealEntry()
        {
            var (solver, performer, target, skill) = SetupDamage();
            skill.Category = SkillCategory.Heal;
            skill.Heal = new HealComponent(5, 5);

            solver.ExecuteSkill(performer, target, skill, null);

            Assert.That(solver.SkillResult.SkillEntries, Has.Count.EqualTo(1));
            Assert.That(solver.SkillResult.SkillEntries[0].Type, Is.EqualTo(SkillResultType.Heal));
            Assert.That(solver.SkillResult.SkillEntries[0].Amount, Is.EqualTo(5));
        }
    }
}