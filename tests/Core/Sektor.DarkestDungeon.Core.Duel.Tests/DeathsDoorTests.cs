using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    [TestFixture]
    public class DeathsDoorTests
    {
        [Test]
        public void ZeroHealth_EntersDeathsDoorInsteadOfDying()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue = 1;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var skill = FirstDamageSkill(duel, attacker, hero);

            duel.ExecuteSkill(attacker, hero, skill);

            Assert.That(hero.Character.AtDeathsDoor, Is.True, "A hero at zero health should enter death's door.");
            Assert.That(hero.CombatInfo.IsDead, Is.False, "The hero should not die on the first hit to zero.");
        }

        [Test]
        public void LowDeathResist_DiesOnSecondHit()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue = 1;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DeathBlow)).RawValue = 0f;

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var skill = FirstDamageSkill(duel, attacker, hero);

            int otherStress = (int)duel.HeroParty.Units[1].Character.Stress.CurrentValue;

            duel.ExecuteSkill(attacker, hero, skill);
            Assert.That(hero.Character.AtDeathsDoor, Is.True);

            duel.ExecuteSkill(attacker, hero, skill);
            Assert.That(hero.CombatInfo.IsDead, Is.True, "A hero with 0 death blow resist should die on the second hit.");
            Assert.That((int)duel.HeroParty.Units[1].Character.Stress.CurrentValue, Is.EqualTo(otherStress + 15),
                "A hero death should stress the surviving party by 15.");
        }

        [Test]
        public void HighDeathResist_SurvivesTheDeathBlowRoll()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue = 1;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DeathBlow)).RawValue = 0.9f;

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var skill = FirstDamageSkill(duel, attacker, hero);

            duel.ExecuteSkill(attacker, hero, skill);
            Assert.That(hero.Character.AtDeathsDoor, Is.True);

            duel.ExecuteSkill(attacker, hero, skill);
            Assert.That(hero.CombatInfo.IsDead, Is.False, "A hero with high death blow resist should survive the roll.");
            Assert.That(hero.Character.AtDeathsDoor, Is.True, "The hero should remain at death's door after surviving.");
        }

        [Test]
        public void Healing_RecoversFromDeathsDoor()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("vestal"), Picks("crusader"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue = 1;
            var healer = duel.HeroParty.Units[1];
            var healSkill = healer.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Heal && duel.GetAvailableTargets(healer, s).Contains(hero));
            Assert.That(healSkill, Is.Not.Null, "The vestal should have a heal skill.");

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;
            var damageSkill = FirstDamageSkill(duel, attacker, hero);

            duel.ExecuteSkill(attacker, hero, damageSkill);
            Assert.That(hero.Character.AtDeathsDoor, Is.True);

            duel.ExecuteSkill(healer, hero, healSkill);

            Assert.That(hero.Character.AtDeathsDoor, Is.False, "Healing should recover the hero from death's door.");
            Assert.That((int)hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.GreaterThan(0));
        }

        [Test]
        public void HeartAttack_OnDeathsDoor_KillsTheHero()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var deathsDoorStatus = (DeathsDoorStatusEffect)hero.Character.GetStatusEffect(StatusType.DeathsDoor);
            deathsDoorStatus.AtDeathsDoor = true;
            hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue = 0;

            duel.Events.AddHeartAttackCheck(hero);

            Assert.That(hero.CombatInfo.IsDead, Is.True,
                "A heart attack at death's door should mark the hero for death.");
        }

        [Test]
        public void HeartAttack_NotOnDeathsDoor_EntersDeathsDoor()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.DeathBlow)).RawValue = 0f;

            duel.Events.AddHeartAttackCheck(hero);

            Assert.That(hero.Character.AtDeathsDoor, Is.True,
                "A heart attack off death's door should bring the hero to death's door.");
        }

        private static CombatSkill FirstDamageSkill(DuelController duel, ICombatUnit attacker, ICombatUnit target)
        {
            return attacker.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.GetAvailableTargets(attacker, s).Contains(target));
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