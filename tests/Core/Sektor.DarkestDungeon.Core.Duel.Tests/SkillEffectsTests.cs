using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
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

        [Test]
        public void BuffIdEffect_AppliesContentBuffToTheTarget()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("grave_robber"), Picks("crusader"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[2];
            var daggers = hero.Character.CurrentCombatSkills.FirstOrDefault(skill => skill.Id == "flashing_daggers");
            Assert.That(daggers, Is.Not.Null, "The grave robber should know flashing_daggers.");

            var target = duel.GetAvailableTargets(hero, daggers).FirstOrDefault();
            Assert.That(target, Is.Not.Null, "flashing_daggers should have a valid target from rank 3.");

            ((SingleAttribute)hero.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            ((SingleAttribute)target.Character.GetSingleAttribute(AttributeType.Debuff)).RawValue = 0f;
            float bleedBefore = ((SingleAttribute)target.Character.GetSingleAttribute(AttributeType.Bleed)).ModifiedValue;

            duel.ExecuteSkill(hero, target, daggers);

            float bleedAfter = ((SingleAttribute)target.Character.GetSingleAttribute(AttributeType.Bleed)).ModifiedValue;
            Assert.That(bleedAfter, Is.EqualTo(bleedBefore - 0.2f).Within(0.001f),
                "flashing_daggers should reduce the target's bleed resistance by 20% (bleed_debuff_1).");
        }

        [Test]
        public void TorchEvents_MutateTheDuelTorch()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            Assert.That(duel.Context.TorchAmount, Is.EqualTo(75));
            duel.Events.IncreaseTorch(10);
            Assert.That(duel.Context.TorchAmount, Is.EqualTo(85), "Increasing the torch should raise it by the amount.");
            duel.Events.DecreaseTorch(200);
            Assert.That(duel.Context.TorchAmount, Is.EqualTo(0), "The torch should clamp at 0.");
            duel.Events.IncreaseTorch(500);
            Assert.That(duel.Context.TorchAmount, Is.EqualTo(100), "The torch should clamp at 100.");
        }

        [Test]
        public void SkillLimit_BlocksFurtherUsesAfterLimit()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var limited = new CombatSkill
            {
                Id = "one_shot",
                Level = 0,
                Type = "melee",
                Accuracy = 1f,
                DamageMod = 0f,
                CritMod = 0f,
                IsCritValid = false,
                LimitPerBattle = 1,
                LaunchRanks = new FormationSet("1"),
                TargetRanks = new FormationSet("1"),
            };
            ((Hero)hero.Character).HeroClass.CombatSkills.Add(limited);

            var target = duel.GetAvailableTargets(hero, limited).FirstOrDefault();
            Assert.That(target, Is.Not.Null, "one_shot should have a valid target from rank 1.");

            Assert.That(duel.IsSkillUsable(hero, limited), Is.True, "The first use should be allowed.");

            duel.ExecuteSkill(hero, target, limited);

            Assert.That(hero.CombatInfo.SkillsUsedInBattle, Does.Contain("one_shot"));
            Assert.That(duel.IsSkillUsable(hero, limited), Is.False, "The per-battle limit should block the second use.");
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