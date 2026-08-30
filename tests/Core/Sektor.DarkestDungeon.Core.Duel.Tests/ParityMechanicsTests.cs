using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    [TestFixture]
    public class ParityMechanicsTests
    {
        [Test]
        public void DotTick_ReducesTheTargetHealthOnItsTurn()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var bleeding = (DamageOverTimeStatusEffect)hero.Character.GetStatusEffect(StatusType.Bleeding);
            bleeding.AddInstanse(4, 2);
            Assert.That(bleeding.IsApplied, Is.True);

            int healthBefore = (int)hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            RunTurnsUntil(duel, hero);

            Assert.That((int)hero.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.EqualTo(healthBefore - 4),
                "The bleed tick should deal its tick damage at the start of the target's turn.");
        }

        [Test]
        public void Stun_SkipsTheTurnAndAppliesRecoveryBuff()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var stun = (IStunStatusEffect)hero.Character.GetStatusEffect(StatusType.Stun);
            stun.StunApplied = true;

            float stunResistBefore = hero.Character.GetSingleAttribute(AttributeType.Stun).ModifiedValue;
            RunTurnsUntil(duel, hero);

            Assert.That(stun.StunApplied, Is.False, "The stun should be consumed when the stunned unit's turn starts.");
            Assert.That(hero.Character.GetSingleAttribute(AttributeType.Stun).ModifiedValue,
                Is.EqualTo(stunResistBefore + 0.4f).Within(0.0001f),
                "A stunned unit should receive the STUNRECOVERYBUFF (+40% stun resist) when the stun wears off.");
        }

        [Test]
        public void Riposte_AttacksThePerformerAfterBeingHit()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("man_at_arms"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var defender = duel.HeroParty.Units[0];
            Assert.That(defender.Character.RiposteSkill, Is.Not.Null, "ManAtArms should own a riposte skill.");
            var riposte = (IRiposteStatusEffect)defender.Character.GetStatusEffect(StatusType.Riposte);
            riposte.RiposteDuration = 2;

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            ((SingleAttribute)defender.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.DefenseRating)).RawValue = 0f;

            var skill = attacker.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.GetAvailableTargets(attacker, s).Contains(defender));
            Assert.That(skill, Is.Not.Null, "The attacker should have a damage skill targeting the defender.");

            int attackerHealthBefore = (int)attacker.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            duel.ExecuteSkill(attacker, defender, skill);

            Assert.That((int)attacker.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.LessThan(attackerHealthBefore),
                "The riposte should hit the attacker back after the defender takes a hit.");
        }

        [Test]
        public void Guard_RedirectsTheAttackToTheGuard()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("man_at_arms"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var guard = duel.HeroParty.Units[0];
            var guardedAlly = duel.HeroParty.Units[1];

            var guardEffect = new GuardEffect(false);
            var effect = new Effect();
            effect.IntegerParams[EffectIntParams.Duration] = 2;
            guardEffect.ApplyInstant(guard, guardedAlly, effect, duel.Context);

            var guardedStatus = (IGuardedStatusEffect)guardedAlly.Character.GetStatusEffect(StatusType.Guarded);
            Assert.That(guardedStatus.IsApplied, Is.True, "The ally should be guarded.");
            Assert.That(guardedStatus.Guard, Is.EqualTo(guard), "The guard should be the guarding unit.");

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var skill = attacker.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.GetAvailableTargets(attacker, s).Contains(guardedAlly));
            Assert.That(skill, Is.Not.Null, "The attacker should have a damage skill targeting the guarded ally.");

            int guardHealthBefore = (int)guard.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            int allyHealthBefore = (int)guardedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;

            duel.ExecuteSkill(attacker, guardedAlly, skill);

            Assert.That((int)guard.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.LessThan(guardHealthBefore),
                "The guard should absorb the damage meant for the guarded ally.");
            Assert.That((int)guardedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.EqualTo(allyHealthBefore),
                "The guarded ally should not take the redirected damage.");
        }

        [Test]
        public void Pull_MovesTheTargetForwardInTheFormation()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var target = duel.MonsterParty.Units[3];
            int rankBefore = target.Rank;
            Assert.That(rankBefore, Is.EqualTo(4));

            duel.Events.Pull(target, 2);

            Assert.That(target.Rank, Is.EqualTo(2), "A pull of 2 should move the unit from rank 4 to rank 2.");
        }

        [Test]
        public void Push_MovesTheTargetBackInTheFormation()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var target = duel.MonsterParty.Units[0];
            Assert.That(target.Rank, Is.EqualTo(1));

            duel.Events.Push(target, 2);

            Assert.That(target.Rank, Is.EqualTo(3), "A push of 2 should move the unit from rank 1 to rank 3.");
        }

        [Test]
        public void Pull_IsBlockedWhileImmobilized()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var target = duel.MonsterParty.Units[3];
            ((FormationUnitInfo)target.CombatInfo).IsImmobilized = true;
            int rankBefore = target.Rank;

            duel.Events.Pull(target, 2);

            Assert.That(target.Rank, Is.EqualTo(rankBefore), "An immobilized unit should not be pulled.");
        }

        [Test]
        public void ManualMove_IsBlockedWhileImmobilized()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            RunTurnsUntil(duel, hero);

            ((FormationUnitInfo)hero.CombatInfo).IsImmobilized = true;
            int rankBefore = hero.Rank;

            string payload = duel.ExecuteLocalMove(rankBefore + 1);

            Assert.That(payload, Is.Null, "An immobilized unit should not be able to move manually.");
            Assert.That(hero.Rank, Is.EqualTo(rankBefore), "The immobilized unit's rank should stay unchanged.");
        }

        [Test]
        public void RemoveConditions_ClearsConditionalBuffsAfterTheSkill()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var hero = duel.HeroParty.Units[0];
            var buff = content.GetBuff("TRINKET_ACC_B1");
            Assert.That(buff, Is.Not.Null, "The TRINKET_ACC_B1 buff should exist in the content.");
            var applied = new BuffInfo(buff, BuffDurationType.Round, BuffSourceType.Condition);
            hero.Character.AddBuff(applied);
            float attackBefore = hero.Character.GetSingleAttribute(AttributeType.AttackRating).ModifiedValue;
            Assert.That(attackBefore, Is.EqualTo(0.04f).Within(0.0001f),
                "The conditional buff should raise the attack rating.");

            var attacker = duel.MonsterParty.Units[0];
            ((SingleAttribute)attacker.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var skill = attacker.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.GetAvailableTargets(attacker, s).Contains(hero));
            Assert.That(skill, Is.Not.Null);

            duel.ExecuteSkill(attacker, hero, skill);

            float attackAfter = hero.Character.GetSingleAttribute(AttributeType.AttackRating).ModifiedValue;
            Assert.That(attackAfter, Is.EqualTo(0f).Within(0.0001f),
                "RemoveConditions should strip conditional buffs after the skill executes.");
        }

        private static void RunTurnsUntil(DuelController duel, ICombatUnit wanted)
        {
            int units = duel.HeroParty.Units.Count + duel.MonsterParty.Units.Count;
            for (int turn = 0; turn <= units; turn++)
            {
                if (duel.IsLocalTurn)
                    duel.ExecuteLocalPass();
                else
                    duel.ApplyRemoteSkill(DuelPayload.PassAction());

                if (duel.CurrentUnit == wanted)
                    break;
            }
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
