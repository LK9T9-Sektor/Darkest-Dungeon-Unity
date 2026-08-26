namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Wpf.Combat;
    using Sektor.DarkestDungeon.Wpf.Networking;

    [TestFixture]
    public class DuelFlowTests
    {
        private static DuelHeroPick[] ToPicks(DuelPartyConfig config)
        {
            var picks = new List<DuelHeroPick>();
            for (int i = 0; i < config.ClassIds.Count; i++)
                picks.Add(new DuelHeroPick(config.ClassIds[i], config.Seeds[i]));
            return picks.ToArray();
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

            var hostConfig = new DuelPartyConfig(
                new[] { "crusader", "crusader", "crusader", "crusader" },
                new[] { 1, 2, 3, 4 });
            var clientConfig = new DuelPartyConfig(
                new[] { "highwayman", "highwayman", "plague_doctor", "vestal" },
                new[] { 5, 6, 7, 8 });

            hostSession.SendPartyConfig(hostConfig);
            clientSession.SendPartyConfig(clientConfig);
            hostSession.SendLoaded();
            clientSession.SendLoaded();

            Assert.That(hostSession.RivalParty, Is.Not.Null);
            Assert.That(clientSession.RivalParty, Is.Not.Null);
            Assert.That(hostSession.IsReady, Is.True);
            Assert.That(clientSession.IsReady, Is.True);

            int hostSeed = DuelSeed.ComputeSessionSeed(new[] { hostSession.LocalPlayerId, clientSession.LocalPlayerId });
            int clientSeed = DuelSeed.ComputeSessionSeed(new[] { clientSession.LocalPlayerId, hostSession.LocalPlayerId });
            Assert.That(hostSeed, Is.EqualTo(clientSeed));

            var hostDuel = new DuelController();
            hostDuel.StartDuel(ToPicks(hostConfig), ToPicks(clientConfig), hostSeed);
            var clientDuel = new DuelController();
            clientDuel.StartDuel(ToPicks(clientConfig), ToPicks(hostConfig), clientSeed);

            Assert.That(hostDuel.IsStarted, Is.True);
            Assert.That(clientDuel.IsStarted, Is.True);
            Assert.That(hostDuel.HeroParty.Units.Count, Is.EqualTo(4));
            Assert.That(clientDuel.MonsterParty.Units.Count, Is.EqualTo(4));

            // Lockstep: host's hero attacks client hero 0. Both sides apply the SAME action
            // from the same seed, so the mirrored client-hero converges.
            var hostAttacker = hostDuel.HeroParty.Units[0];
            var hostVictim = hostDuel.MonsterParty.Units[0];
            var clientAttacker = clientDuel.MonsterParty.Units[0];
            var clientVictim = clientDuel.HeroParty.Units[0];
            var skill = ((Sektor.DarkestDungeon.Core.Combat.Character.Hero)hostAttacker.Character).CurrentCombatSkills[0];

            RandomSolver.SetRandomSeed(hostSeed);
            hostDuel.ExecuteSkill(hostAttacker, hostVictim, skill);
            RandomSolver.SetRandomSeed(clientSeed);
            clientDuel.ExecuteSkill(clientAttacker, clientVictim, skill);

            Assert.That(hostDuel.MonsterParty.Units[0].Character.HealthRatio,
                Is.EqualTo(clientDuel.HeroParty.Units[0].Character.HealthRatio).Within(0.0001f));
        }
    }
}