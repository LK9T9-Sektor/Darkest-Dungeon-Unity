namespace Sektor.DarkestDungeon.Lan.Tests.Transport
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Lan.Contracts.Transport;
    using Sektor.DarkestDungeon.Lan.Tests.Support;

    [TestFixture]
    public class MessageRoundTripTests
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
            _host.CreateSession("room-1", 8);
            _client.JoinSession("room-1");
        }

        [Test]
        public void DeliversMessageToPeer_WithSenderStamp()
        {
            List<TransportMessage> received = new List<TransportMessage>();
            _client.MessageReceived += message => received.Add(message);

            var result = _host.SendMessage("hero_skill_selected", "{\"slot\":2}");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(received, Has.Count.EqualTo(1));
            Assert.That(received[0].SenderId, Is.EqualTo("player-host"));
            Assert.That(received[0].Type, Is.EqualTo("hero_skill_selected"));
            Assert.That(received[0].Payload, Is.EqualTo("{\"slot\":2}"));
        }

        [Test]
        public void DeliversMessagesInOrder()
        {
            List<string> receivedTypes = new List<string>();
            _client.MessageReceived += message => receivedTypes.Add(message.Type);

            _host.SendMessage("first", "1");
            _host.SendMessage("second", "2");
            _host.SendMessage("third", "3");

            Assert.That(receivedTypes, Is.EqualTo(new[] { "first", "second", "third" }));
        }

        [Test]
        public void GetSessionPlayers_ExcludesLocalPlayer()
        {
            string[] hostPlayers = _host.GetSessionPlayers();

            Assert.That(hostPlayers, Is.EquivalentTo(new[] { "player-client" }));
        }
    }
}
