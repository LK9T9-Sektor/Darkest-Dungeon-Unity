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
    using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

    [TestFixture]
    public class DeterminismTests
    {
        [TearDown]
        public void ResetSeed()
        {
            RandomSolver.SetRandomSeed(0);
        }

        private static HeroClass BuildSampleClass()
        {
            var heroClass = new HeroClass { StringId = "crusader" };
            heroClass.Attributes[AttributeType.HitPoints] = 40;
            heroClass.Attributes[AttributeType.SpeedRating] = 0;
            heroClass.Attributes[AttributeType.AttackRating] = 0;
            heroClass.Attributes[AttributeType.CritChance] = 0;
            heroClass.Attributes[AttributeType.DamageLow] = 5;
            heroClass.Attributes[AttributeType.DamageHigh] = 8;
            heroClass.Attributes[AttributeType.DefenseRating] = 0;
            heroClass.Attributes[AttributeType.ProtectionRating] = 0;

            var skill = new CombatSkill
            {
                Id = "smite",
                Category = SkillCategory.Damage,
                Accuracy = 0.9f,
                DamageMin = 5,
                DamageMax = 8,
                DamageMod = 0,
                CritMod = 0,
                IsCritValid = false,
                CanMiss = null,
                LaunchRanks = new FormationSet("1234"),
                TargetRanks = new FormationSet("1234"),
            };
            heroClass.CombatSkills.Add(skill);
            return heroClass;
        }

        private static (BattleSolver Solver, FormationUnit Attacker, FormationUnit Target, CombatSkill Skill) SetupDuel(
            bool heroAttacker)
        {
            var heroClass = BuildSampleClass();
            var attacker = HeroGeneration.GenerateHero(heroClass, 42);
            var target = HeroGeneration.GenerateHero(heroClass, 43);

            var attackerParty = new FormationParty();
            var targetParty = new FormationParty();
            var attackerUnit = new FormationUnit(attacker, heroAttacker ? Team.Heroes : Team.Monsters);
            var targetUnit = new FormationUnit(target, heroAttacker ? Team.Monsters : Team.Heroes);
            attackerParty.AddUnit(attackerUnit);
            targetParty.AddUnit(targetUnit);
            attackerUnit.PrepareForBattle(1);
            targetUnit.PrepareForBattle(2);

            var battleGround = new BattleGround(
                heroAttacker ? attackerParty : targetParty,
                heroAttacker ? targetParty : attackerParty);

            var events = Substitute.For<IBattleEvents>();
            var battleContext = Substitute.For<IBattleContext>();
            battleContext.BattleGround.Returns(battleGround);
            battleContext.Events.Returns(events);
            battleContext.GetSkillAvailableTargets(Arg.Any<ICombatUnit>(), Arg.Any<CombatSkill>())
                .Returns(x => new BattleSolver(battleContext).GetSkillAvailableTargets((ICombatUnit)x[0], (CombatSkill)x[1]));
            battleContext.IsSkillUsable(Arg.Any<ICombatUnit>(), Arg.Any<CombatSkill>())
                .Returns(x => new BattleSolver(battleContext).IsSkillUsable((ICombatUnit)x[0], (CombatSkill)x[1]));

            return (new BattleSolver(battleContext), attackerUnit, targetUnit, heroClass.CombatSkills[0]);
        }

        [Test]
        public void ExecuteSkill_SameSeed_ProducesIdenticalResult()
        {
            var (solverA, attackerA, targetA, skill) = SetupDuel(heroAttacker: true);
            var (solverB, attackerB, targetB, _) = SetupDuel(heroAttacker: true);

            RandomSolver.SetRandomSeed(1234);
            solverA.ExecuteSkill(attackerA, targetA, skill, null);

            RandomSolver.SetRandomSeed(1234);
            solverB.ExecuteSkill(attackerB, targetB, skill, null);

            Assert.That(solverB.SkillResult.SkillEntries[0].Type, Is.EqualTo(solverA.SkillResult.SkillEntries[0].Type));
            Assert.That(solverB.SkillResult.SkillEntries[0].Amount, Is.EqualTo(solverA.SkillResult.SkillEntries[0].Amount));
            Assert.That(targetB.Character.HealthRatio, Is.EqualTo(targetA.Character.HealthRatio).Within(0.0001f));
        }

        [Test]
        public void GenerateHero_SameSeed_ProducesIdenticalHero()
        {
            var heroClass = BuildSampleClass();
            var first = HeroGeneration.GenerateHero(heroClass, 99);
            var second = HeroGeneration.GenerateHero(heroClass, 99);

            Assert.That(second.Name, Is.EqualTo(first.Name));
            Assert.That(second.MaxHealth, Is.EqualTo(first.MaxHealth));
        }
    }
}