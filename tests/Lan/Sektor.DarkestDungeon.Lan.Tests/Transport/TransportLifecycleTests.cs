using System.Collections.Generic;

using NUnit.Framework;

using Sektor.DarkestDungeon.Lan.Tests.Support;

namespace Sektor.DarkestDungeon.Lan.Tests.Transport
{
    [TestFixture]
    public class TransportLifecycleTests
    {
        private InMemoryTransport _host;
        private InMemoryTransport _client;

        [SetUp]
        public void SetUp()
        {
            _host = new InMemoryTransport("player-host");
            _client = new InMemoryTransport("player-client");
            _host.LinkTo(_client);
            _client.LinkTo(_host);
        }

        [Test]
        public void CreateSession_MarksHostActive_AndRaisesSessionJoined()
        {
            List<string> joinedSessions = new List<string>();
            _host.SessionJoined += sessionId => joinedSessions.Add(sessionId);

            var result = _host.CreateSession("room-1", 8);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_host.IsSessionActive, Is.True);
            Assert.That(joinedSessions, Is.EquivalentTo(new[] { "room-1" }));
        }

        [Test]
        public void CreateSession_WhenAlreadyActive_Fails()
        {
            _host.CreateSession("room-1", 8);

            var result = _host.CreateSession("room-2", 8);

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void JoinSession_RaisesSessionJoinedOnJoiner_AndPlayerJoinedOnBothSides()
        {
            _host.CreateSession("room-1", 8);
            List<string> hostJoined = new List<string>();
            List<string> clientJoined = new List<string>();
            _host.PlayerJoined += playerId => hostJoined.Add(playerId);
            _client.PlayerJoined += playerId => clientJoined.Add(playerId);

            var result = _client.JoinSession("room-1");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_client.IsSessionActive, Is.True);
            Assert.That(hostJoined, Is.EquivalentTo(new[] { "player-client" }));
            Assert.That(clientJoined, Is.EquivalentTo(new[] { "player-host" }));
        }

        [Test]
        public void JoinSession_WhenHostHasNoSession_Fails()
        {
            var result = _client.JoinSession("room-1");

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(_client.IsSessionActive, Is.False);
        }

        [Test]
        public void LeaveSession_RaisesPlayerLeftOnPeer()
        {
            _host.CreateSession("room-1", 8);
            _client.JoinSession("room-1");
            List<string> hostLeft = new List<string>();
            _host.PlayerLeft += playerId => hostLeft.Add(playerId);

            var result = _client.LeaveSession();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_client.IsSessionActive, Is.False);
            Assert.That(hostLeft, Is.EquivalentTo(new[] { "player-client" }));
        }

        [Test]
        public void HostPlayerId_IsSetOnHost_AndPropagatesToJoiner()
        {
            _host.CreateSession("room-1", 8);

            Assert.That(_host.HostPlayerId, Is.EqualTo("player-host"));
            Assert.That(_client.HostPlayerId, Is.Empty);

            _client.JoinSession("room-1");

            Assert.That(_client.HostPlayerId, Is.EqualTo("player-host"));
        }

        [Test]
        public void SendMessage_WhenNotInSession_Fails()
        {
            var result = _client.SendMessage("type", "payload");

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void InviteReceived_RaisesSessionInviteReceivedWithSessionId()
        {
            List<string> invites = new List<string>();
            _client.SessionInviteReceived += sessionId => invites.Add(sessionId);

            _client.NotifyInviteReceived("room-42");

            Assert.That(invites, Is.EquivalentTo(new[] { "room-42" }));
        }
    }
}
