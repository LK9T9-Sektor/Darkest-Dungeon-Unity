using System;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests that active status effects (bleed, stun, mark, guard) surface in the card table.</summary>
    [TestFixture]
    public class StatusTableTests
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
        public void ActiveBleedStatus_ShowsInTheDebuffColumn()
        {
            var duel = CreateDuel();
            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });
            view.Refresh();
            int combatId = view.Heroes[0].CombatId;
            var unit = duel.HeroParty.Units.First(u => u.CombatInfo.CombatId == combatId);
            ((IDotStatusEffect)unit.Character.GetStatusEffect(StatusType.Bleeding)).AddInstanse(3, 2);

            view.Refresh();

            var card = view.Heroes.First(c => c.CombatId == combatId);
            Assert.That(card.Debuffs, Has.Count.EqualTo(1));
            Assert.That(card.Debuffs[0].Name, Is.EqualTo("Bleeding"));
            Assert.That(card.Debuffs[0].DurationText, Is.EqualTo("2 rounds"));
        }

        [Test]
        public void ActiveStunStatus_ShowsInTheDebuffColumn()
        {
            var duel = CreateDuel();
            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });
            view.Refresh();
            int combatId = view.Heroes[0].CombatId;
            var unit = duel.HeroParty.Units.First(u => u.CombatInfo.CombatId == combatId);
            ((IStunStatusEffect)unit.Character.GetStatusEffect(StatusType.Stun)).StunApplied = true;

            view.Refresh();

            var card = view.Heroes.First(c => c.CombatId == combatId);
            Assert.That(card.Debuffs, Has.Count.EqualTo(1));
            Assert.That(card.Debuffs[0].Name, Is.EqualTo("Stunned"));
        }

        [Test]
        public void AppliedBuff_ShowsInTheBuffColumn()
        {
            var duel = CreateDuel();
            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });
            view.Refresh();
            int combatId = view.Heroes[0].CombatId;
            var unit = duel.HeroParty.Units.First(u => u.CombatInfo.CombatId == combatId);
            unit.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f),
                BuffDurationType.Round, BuffSourceType.Adventure, 3));

            view.Refresh();

            var card = view.Heroes.First(c => c.CombatId == combatId);
            Assert.That(card.Buffs, Has.Count.EqualTo(1));
            Assert.That(card.Buffs[0].Name, Is.EqualTo("Accuracy"));
            Assert.That(card.Buffs[0].Description, Is.EqualTo("+6% Accuracy"));
            Assert.That(card.Buffs[0].DurationText, Is.EqualTo("3 rounds"));
        }

        private static DuelController CreateDuel()
        {
            var duel = new DuelController(new DuelContent());
            duel.StartDuel(Picks(1), Picks(5), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();
            return duel;
        }

        private static DuelHeroPick[] Picks(int firstSeed)
        {
            return new[]
            {
                new DuelHeroPick("crusader", firstSeed),
                new DuelHeroPick("crusader", firstSeed + 1),
                new DuelHeroPick("crusader", firstSeed + 2),
                new DuelHeroPick("crusader", firstSeed + 3),
            };
        }
    }
}