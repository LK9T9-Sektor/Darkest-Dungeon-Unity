using System;
using System.Collections.Generic;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies that both parties may mix heroes and monsters and that the shared duel view renders and runs such a fight.</summary>
    [TestFixture]
    public class MixedPartyBattleTests
    {
        [Test]
        public void MixedSides_StartRunAndRender()
        {
            var player = new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 1, new List<string> { "smite" }, null),
                new MonsterFightUnitSpec("cultist_brawler_A"),
            };
            var ai = new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("vestal", 2, new List<string> { "judgement" }, null),
                new MonsterFightUnitSpec("cultist_witch_A"),
            };

            var duel = new DuelController(new DuelContent());
            duel.StartFight(player, ai, 42);
            duel.SetLocalTeam(Team.Heroes);
            duel.StartBattle();

            var view = new DuelBattleViewModel(duel, new NullLink(), () => { });
            Assert.That(view.Heroes.Count, Is.EqualTo(2), "The player side renders both its hero and monster.");
            Assert.That(view.Monsters.Count, Is.EqualTo(2), "The AI side renders both its hero and monster.");

            var fallback = new FallbackDuelAi();
            int steps = 0;
            while (!duel.IsFinished && steps++ < 20000)
            {
                if (duel.IsLocalTurn)
                    duel.ExecuteLocalPass();
                else
                    duel.ApplyRemoteSkill(fallback.ChooseAction(duel));
            }

            Assert.That(duel.IsFinished, Is.True);
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
