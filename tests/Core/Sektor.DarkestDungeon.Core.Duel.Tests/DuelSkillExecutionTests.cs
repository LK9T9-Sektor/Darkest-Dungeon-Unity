using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    /// <summary>Tests for the duel skill execution path: target validation and multi-target (AOE) resolution.</summary>
    [TestFixture]
    public class DuelSkillExecutionTests
    {
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

        private static DuelController CreateDuel(string hostClass, string clientClass)
        {
            var duel = new DuelController(new TestDuelContent());
            duel.StartDuel(Picks(hostClass), Picks(clientClass), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();
            return duel;
        }

        private static ICombatUnit AdvanceToLocalDamageTurn(DuelController duel)
        {
            int units = duel.HeroParty.Units.Count + duel.MonsterParty.Units.Count;
            for (int turn = 0; turn <= units * 2 && !duel.IsFinished; turn++)
            {
                if (duel.IsLocalTurn)
                {
                    if (FirstUsableDamageSkill(duel, duel.CurrentUnit) != null)
                        return duel.CurrentUnit;
                    duel.ExecuteLocalPass();
                }
                else
                {
                    duel.ApplyRemoteSkill(DuelPayload.PassAction());
                }
            }
            return null;
        }

        private static CombatSkill FirstUsableDamageSkill(DuelController duel, ICombatUnit unit)
        {
            return unit.Character.CurrentCombatSkills.FirstOrDefault(
                s => s.Category == SkillCategory.Damage && duel.IsSkillUsable(unit, s));
        }

        [Test]
        public void ExecuteLocalSkill_SelfAttack_IsRejected()
        {
            var duel = CreateDuel("crusader", "highwayman");
            var unit = AdvanceToLocalDamageTurn(duel);
            Assert.That(unit, Is.Not.Null, "A crusader with a usable damage skill should eventually get a turn.");

            var skill = FirstUsableDamageSkill(duel, unit);

            float healthBefore = unit.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            string payload = duel.ExecuteLocalSkill(skill.Id, unit.CombatInfo.CombatId);

            Assert.That(payload, Is.Null, "An attacking skill must not be executable against the performer itself.");
            Assert.That(unit.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.EqualTo(healthBefore));
            Assert.That(duel.CurrentUnit, Is.SameAs(unit), "A rejected self-attack must not end the turn.");
        }

        [Test]
        public void ExecuteLocalSkill_AllyAttack_IsRejected()
        {
            var duel = CreateDuel("crusader", "highwayman");
            var unit = AdvanceToLocalDamageTurn(duel);
            Assert.That(unit, Is.Not.Null, "A crusader with a usable damage skill should eventually get a turn.");

            var skill = FirstUsableDamageSkill(duel, unit);

            var ally = duel.HeroParty.Units.FirstOrDefault(candidate => candidate != unit);
            Assert.That(ally, Is.Not.Null);

            float allyHealthBefore = ally.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            string payload = duel.ExecuteLocalSkill(skill.Id, ally.CombatInfo.CombatId);

            Assert.That(payload, Is.Null, "An attacking skill must not be executable against a friendly unit.");
            Assert.That(ally.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue, Is.EqualTo(allyHealthBefore));
        }

        [Test]
        public void ExecuteSkill_MultiTarget_AoeResolvesEveryEnemyInTheRanks()
        {
            var duel = CreateDuel("hellion", "highwayman");

            var hellion = duel.HeroParty.Units[0];
            var breakthru = hellion.Character.CurrentCombatSkills.FirstOrDefault(s => s.Id == "breakthru");
            Assert.That(breakthru, Is.Not.Null, "The hellion should own the breakthru AOE skill.");

            var enemies = duel.MonsterParty.Units;
            var aoeTargets = enemies.Where(u => u.Rank <= 3).ToList();
            Assert.That(aoeTargets.Count, Is.EqualTo(3), "breakthru hits enemy ranks 1-3 (all present).");
            var primary = aoeTargets.First(u => u.Rank == 1);

            duel.ExecuteSkill(hellion, primary, breakthru);

            var entries = duel.Solver.SkillResult.SkillEntries;
            Assert.That(entries.Count, Is.EqualTo(3), "The AOE skill must resolve against every enemy in its ranks.");
            Assert.That(entries.All(entry => aoeTargets.Contains(entry.Target)), Is.True,
                "Every resolved entry targets an enemy within the AOE ranks.");
        }

        [Test]
        public void ExecuteSkill_MultiTarget_PartyHealHealsEveryAlly()
        {
            var duel = CreateDuel("vestal", "highwayman");

            var vestal = duel.HeroParty.Units[0];
            var godsComfort = vestal.Character.CurrentCombatSkills.FirstOrDefault(s => s.Id == "gods_comfort");
            Assert.That(godsComfort, Is.Not.Null, "The vestal should own the gods_comfort party-heal skill.");

            var woundedAlly = duel.HeroParty.Units[1];
            woundedAlly.Character.TakeDamage(4);
            float healthBefore = woundedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;

            duel.ExecuteSkill(vestal, woundedAlly, godsComfort);

            var allies = duel.HeroParty.Units;
            var entries = duel.Solver.SkillResult.SkillEntries;
            Assert.That(entries.Count, Is.EqualTo(allies.Count), "The party heal must resolve against every ally.");
            Assert.That(entries.All(entry => allies.Contains(entry.Target)), Is.True,
                "Every resolved entry targets an ally.");
            Assert.That(woundedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                Is.GreaterThan(healthBefore), "The wounded ally must actually be healed.");
        }

        [Test]
        public void ExecuteSkill_BuffSkill_AppliesTheBuffToTheTarget()
        {
            var duel = CreateDuel("plague_doctor", "highwayman");

            var doctor = duel.HeroParty.Units[0];
            var embolden = doctor.Character.CurrentCombatSkills.FirstOrDefault(s => s.Id == "emboldening_vapours");
            Assert.That(embolden, Is.Not.Null, "The plague doctor should own the emboldening_vapours buff skill.");

            var ally = duel.HeroParty.Units[1];
            int speedBefore = (int)ally.Character.Speed;

            duel.ExecuteSkill(doctor, ally, embolden);

            var source = ally.Character as Character;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.BuffInfos.Count, Is.GreaterThan(0),
                "The buff skill must apply a stat buff to the target ally.");
            Assert.That((int)ally.Character.Speed, Is.GreaterThan(speedBefore),
                "The applied speed buff must raise the ally's speed.");
        }

        [Test]
        public void ExecuteSkill_SingleTargetHeal_HealsOnlyTheClickedAlly()
        {
            var duel = CreateDuel("vestal", "highwayman");

            var vestal = duel.HeroParty.Units[0];
            var divineGrace = vestal.Character.CurrentCombatSkills.FirstOrDefault(s => s.Id == "divine_grace");
            Assert.That(divineGrace, Is.Not.Null, "The vestal should own the divine_grace single-target heal.");

            var clickedAlly = duel.HeroParty.Units[1];
            var bystanderAlly = duel.HeroParty.Units[2];
            clickedAlly.Character.TakeDamage(6);
            bystanderAlly.Character.TakeDamage(6);
            float clickedBefore = clickedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;
            float bystanderBefore = bystanderAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue;

            duel.ExecuteSkill(vestal, clickedAlly, divineGrace);

            Assert.That(clickedAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                Is.GreaterThan(clickedBefore), "The clicked ally must be healed.");
            Assert.That(bystanderAlly.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue,
                Is.EqualTo(bystanderBefore), "A single-target heal must not touch the other wounded ally.");
        }

        [Test]
        public void ExecuteSkill_PullSkill_MovesTheEnemyForward()
        {
            var duel = CreateDuel("occultist", "highwayman");

            var occultist = duel.HeroParty.Units[0];
            var daemonsPull = occultist.Character.CurrentCombatSkills.FirstOrDefault(s => s.Id == "daemons_pull");
            Assert.That(daemonsPull, Is.Not.Null, "The occultist should own the daemons_pull skill.");

            ((SingleAttribute)occultist.Character.GetSingleAttribute(AttributeType.AttackRating)).RawValue = 1.0f;
            var enemy = duel.MonsterParty.Units.FirstOrDefault(u => u.Rank >= 3);
            Assert.That(enemy, Is.Not.Null);
            ((SingleAttribute)enemy.Character.GetSingleAttribute(AttributeType.Move)).RawValue = 0f;
            int rankBefore = enemy.Rank;

            RandomSolver.SetRandomSeed(0);
            duel.ExecuteSkill(occultist, enemy, daemonsPull);

            Assert.That(enemy.Rank, Is.LessThan(rankBefore),
                "A successful pull must move the enemy towards the front ranks.");
        }
    }
}