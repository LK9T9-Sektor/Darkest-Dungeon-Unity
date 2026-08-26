namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Wpf.Combat;
    using Sektor.DarkestDungeon.Wpf.Networking;

    [TestFixture]
    public class DuelFlowTests
    {
        private static DuelHeroPick[] Picks(params int[] seeds)
        {
            var picks = new DuelHeroPick[seeds.Length];
            for (int i = 0; i < seeds.Length; i++)
                picks[i] = new DuelHeroPick("crusader", seeds[i]);
            return picks;
        }

        [Test]
        public void FullDuelFlow_BothSides_ConvergeToIdenticalState()
        {
            var hostTransport = new InMemoryTransport("host");
            var clientTransport = new InMemoryTransport("client");
            hostTransport.LinkTo(clientTransport);
            clientTransport.LinkTo(hostTransport);

            var hostSession = new DuelSessionManager(hostTransport);
            var clientSession = new DuelSessionManager(clientTransport);
            hostSession.Start();
            clientSession.Start();

            Assert.That(hostSession.HostSession("duel").IsSuccess, Is.True);
            Assert.That(clientSession.JoinSession("duel").IsSuccess, Is.True);

            var hostConfig = new DuelPartyConfig(new[] { "crusader", "crusader", "crusader", "crusader" }, new[] { 1, 2, 3, 4 });
            var clientConfig = new DuelPartyConfig(new[] { "highwayman", "highwayman", "plague_doctor", "vestal" }, new[] { 5, 6, 7, 8 });

            hostSession.SendPartyConfig(hostConfig);
            clientSession.SendPartyConfig(clientConfig);
            hostSession.SendLoaded();
            clientSession.SendLoaded();

            Assert.That(hostSession.IsReady, Is.True);
            Assert.That(clientSession.IsReady, Is.True);

            int hostSeed = DuelSeed.ComputeSessionSeed(new[] { hostSession.LocalPlayerId, clientSession.LocalPlayerId });
            int clientSeed = DuelSeed.ComputeSessionSeed(new[] { clientSession.LocalPlayerId, hostSession.LocalPlayerId });
            Assert.That(hostSeed, Is.EqualTo(clientSeed));

            var hostDuel = new DuelController();
            hostDuel.StartDuel(Picks(1, 2, 3, 4), Picks(5, 6, 7, 8), hostSeed, isHost: true);
            var clientDuel = new DuelController();
            clientDuel.StartDuel(Picks(1, 2, 3, 4), Picks(5, 6, 7, 8), clientSeed, isHost: false);

            Assert.That(hostDuel.HeroParty.Units.Count, Is.EqualTo(4));
            Assert.That(hostDuel.MonsterParty.Units.Count, Is.EqualTo(4));
            Assert.That(clientDuel.HeroParty.Units.Count, Is.EqualTo(4));
            Assert.That(clientDuel.MonsterParty.Units.Count, Is.EqualTo(4));

            RandomSolver.SetRandomSeed(hostSeed);
            hostDuel.StartBattle();
            RandomSolver.SetRandomSeed(clientSeed);
            clientDuel.StartBattle();

            Assert.That(hostDuel.Phase, Is.EqualTo(clientDuel.Phase));
            Assert.That(hostDuel.CurrentUnit!.CombatInfo.CombatId, Is.EqualTo(clientDuel.CurrentUnit!.CombatInfo.CombatId));
        }
    }
}