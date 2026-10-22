using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Monster parties: verifies size-aware ranks and that the shared duel view runs a
    /// heroes-vs-monsters fight through the shared <see cref="DuelBattleViewModel"/>.</summary>
    [TestFixture]
    public class MonsterPartyBattleTests
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

        private static DuelBattleViewModel CreateFight(int seed, out DuelController duel, params string[] monsterIds)
        {
            var heroes = StandardHeroes();
            var monsters = monsterIds.Select(id => (FightUnitSpec)new MonsterFightUnitSpec(id)).ToList();

            duel = new DuelController(new DuelContent());
            duel.StartFight(heroes, monsters, seed);
            duel.SetLocalTeam(Team.Heroes);
            duel.StartBattle();
            return new DuelBattleViewModel(duel, new NullLink(), () => { });
        }

        private static void DriveUntilFinished(DuelController duel)
        {
            var fallback = new FallbackDuelAi();
            int steps = 0;
            while (!duel.IsFinished && steps++ < StepBudget)
            {
                if (duel.IsLocalTurn)
                    duel.ExecuteLocalPass();
                else
                    duel.ApplyRemoteSkill(fallback.ChooseAction(duel));
            }
        }

        [Test]
        public void SizeTwoMonster_OccupiesTwoRanksAndRendersWider()
        {
            var view = CreateFight(42, out var duel, "ghoul_A", "cultist_brawler_A");

            Assert.That(duel.MonsterParty.Units[0].Rank, Is.EqualTo(1), "The size-2 monster starts at rank 1.");
            Assert.That(duel.MonsterParty.Units[0].Size, Is.EqualTo(2));
            Assert.That(duel.MonsterParty.Units[1].Rank, Is.EqualTo(3), "The next monster starts after rank 2.");

            var big = view.Monsters.First(m => m.Rank == 1);
            var small = view.Monsters.First(m => m.Rank == 3);
            Assert.That(big.Size, Is.EqualTo(2));
            Assert.That(big.CardWidth, Is.EqualTo(370), "The card is 185px per occupied rank.");
            Assert.That(small.Size, Is.EqualTo(1));
            Assert.That(small.CardWidth, Is.EqualTo(185));
            view.Leave();
        }

        [Test]
        public void Fight_WithSizeTwoMonster_FinishesWithinBudget()
        {
            var view = CreateFight(42, out var duel, "ghoul_A", "cultist_brawler_A", "cultist_witch_A");
            DriveUntilFinished(duel);

            Assert.That(duel.IsFinished, Is.True, "The fight must finish within the step budget.");
            view.Leave();
        }

        [Test]
        public void Fight_WithBossSizedMonster_FinishesWithoutExceptions()
        {
            var view = CreateFight(42, out var duel, "hag_A");
            DriveUntilFinished(duel);

            Assert.That(duel.IsFinished, Is.True, "A single boss-sized monster fight must finish.");
            view.Leave();
        }

        private sealed class NullLink : IDuelRivalLink
        {
            public event Action<string>? RivalActionReceived;
            public event Action<string>? SkillPreviewed;
            public event Action<int>? TargetPreviewed;

            public void SendLocalAction(string payload)
            {
            }

            public void Attach(DuelController controller)
            {
            }

            public void Detach()
            {
            }

            public void Pump()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
