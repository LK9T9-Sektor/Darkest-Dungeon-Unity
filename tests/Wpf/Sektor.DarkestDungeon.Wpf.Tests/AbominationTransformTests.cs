using System;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Regression tests for the Abomination mode (transform) through the WPF view model.</summary>
    [TestFixture]
    public class AbominationTransformTests
    {
        private sealed class NullRivalLink : IDuelRivalLink
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

        [Test]
        public void TransformSkill_SwitchesToBeast_WithoutKeyNotFound()
        {
            var duel = CreateAbominationDuel();
            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });

            if (!duel.IsLocalTurn)
                Assert.Pass("First unit is the rival; transform is covered on the local side.");

            var actor = duel.CurrentUnit!;
            var transform = actor.Character.CurrentCombatSkills!.Single(skill => skill.Id == "transform");

            Assert.That(actor.Character.CurrentMode, Is.Not.Null);
            Assert.That(actor.Character.CurrentMode!.Id, Is.EqualTo("human"));
            Assert.That(transform.ValidModes, Does.Contain("human"));
            Assert.That(transform.ModeEffects.ContainsKey("human"), Is.True,
                "The transform skill must carry its human-mode effects.");

            string payload = duel.ExecuteLocalSkill(transform.Id, actor.CombatInfo.CombatId);
            view.Refresh();

            Assert.That(payload, Is.Not.Null, "transform should execute without throwing.");
            Assert.That(actor.Character.CurrentMode!.Id, Is.EqualTo("beast"),
                "Transform should switch the hero to beast mode.");
        }

        private static DuelController CreateAbominationDuel()
        {
            var duel = new DuelController(new DuelContent());
            duel.StartDuel(Picks("abomination"), Picks("crusader"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();
            return duel;
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