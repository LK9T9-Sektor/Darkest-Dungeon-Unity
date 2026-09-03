using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>
    /// PvE battle: a heroes-vs-monsters fight driven through <see cref="PveBattleViewModel"/>. Verifies
    /// the monster side renders with its occupied ranks (size-aware), the player's hero side acts via
    /// the remote path and the battle runs to a winner without exceptions.
    /// </summary>
    [TestFixture]
    public class PveBattleTests
    {
        private const int StepBudget = 20000;

        private static List<FightUnitSpec> StandardHeroes()
        {
            return new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 101, new List<string> { "smite", "zealous_accusation", "stunning_blow", "battle_heal" }, null),
                new HeroFightUnitSpec("vestal", 102, new List<string> { "judgement", "mace_bash", "divine_grace", "dazzling_light" }, null),
                new HeroFightUnitSpec("highwayman", 103, new List<string> { "wicked_slice", "pistol_shot", "point_blank_shot", "opened_vein" }, null),
                new HeroFightUnitSpec("plague_doctor", 104, new List<string> { "noxious_blast", "plague_grenade", "incision", "battlefield_medicine" }, null),
            };
        }

        private static PveBattleViewModel CreateFight(int seed, params string[] monsterIds)
        {
            var heroes = StandardHeroes();
            var monsters = monsterIds.Select(id => (FightUnitSpec)new MonsterFightUnitSpec(id)).ToList();

            var duel = new DuelController(new DuelContent());
            duel.StartFight(heroes, monsters, seed);
            RandomSolver.SetRandomSeed(seed);
            duel.StartBattle();
            return new PveBattleViewModel(duel, () => { });
        }

        private static void DriveUntilFinished(PveBattleViewModel view)
        {
            int steps = 0;
            while (!view.Duel.IsFinished && steps < StepBudget)
            {
                view.Pump();
                if (view.IsLocalTurn)
                    DrivePlayerSkill(view);
                steps++;
            }
        }

        private static void DrivePlayerSkill(PveBattleViewModel view)
        {
            view.Refresh();
            if (!view.IsLocalTurn)
                return;

            foreach (var skill in view.Skills.Where(s => s.IsUsable))
            {
                view.SelectSkillCommand.Execute(skill);
                var target = view.Monsters.FirstOrDefault(m => m.IsTarget);
                if (target == null)
                    continue;
                view.TargetCommand.Execute(target);
                return;
            }

            view.PassCommand.Execute(null);
        }

        [Test]
        public void SizeTwoMonster_OccupiesTwoRanksAndRendersWider()
        {
            var view = CreateFight(42, "ghoul_A", "cultist_brawler_A");

            Assert.That(view.Duel.MonsterParty.Units[0].Rank, Is.EqualTo(1), "The size-2 monster starts at rank 1.");
            Assert.That(view.Duel.MonsterParty.Units[0].Size, Is.EqualTo(2));
            Assert.That(view.Duel.MonsterParty.Units[1].Rank, Is.EqualTo(3), "The next monster starts after rank 2.");

            var big = view.Monsters.First(m => m.Rank == 1);
            var small = view.Monsters.First(m => m.Rank == 3);
            Assert.That(big.Size, Is.EqualTo(2));
            Assert.That(big.CardWidth, Is.EqualTo(370), "The card is 185px per occupied rank.");
            Assert.That(small.Size, Is.EqualTo(1));
            Assert.That(small.CardWidth, Is.EqualTo(185));
        }

        [Test]
        public void Fight_WithSizeTwoMonster_FinishesWithinBudget()
        {
            var view = CreateFight(42, "ghoul_A", "cultist_brawler_A", "cultist_witch_A");
            DriveUntilFinished(view);

            Assert.That(view.Duel.IsFinished, Is.True, "The PvE fight must finish within the step budget.");
            Assert.That(view.Status, Does.Contain("WIN"));
        }

        [Test]
        public void Fight_WithBossSizedMonster_FinishesWithoutExceptions()
        {
            var view = CreateFight(42, "hag_A");
            DriveUntilFinished(view);

            Assert.That(view.Duel.IsFinished, Is.True, "A single boss-sized monster fight must finish.");
        }
    }
}