namespace Sektor.DarkestDungeon.Lan.Tests.Codec
{
    using NUnit.Framework;

    using Sektor.DarkestDungeon.Lan.Contracts.Transport;
    using Sektor.DarkestDungeon.Lan.Steam;

    [TestFixture]
    public class JsonTransportCodecTests
    {
        private JsonTransportCodec _codec;

        [SetUp]
        public void SetUp()
        {
            _codec = new JsonTransportCodec();
        }

        [Test]
        public void RoundTrip_PreservesTypeAndPayload()
        {
            var original = new TransportMessage("sender-a", "hero_skill_selected", "{\"slot\":2}");

            string text = _codec.Serialize(original);
            TransportMessage decoded = _codec.Deserialize(text);

            Assert.That(decoded.Type, Is.EqualTo("hero_skill_selected"));
            Assert.That(decoded.Payload, Is.EqualTo("{\"slot\":2}"));
        }

        [Test]
        public void RoundTrip_DoesNotCarrySender()
        {
            var original = new TransportMessage("sender-a", "type", "payload");

            string text = _codec.Serialize(original);
            TransportMessage decoded = _codec.Deserialize(text);

            Assert.That(decoded.SenderId, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Serialize_ProducesJsonWithTypeAndPayloadKeys()
        {
            var original = new TransportMessage("sender-a", "type", "payload");

            string text = _codec.Serialize(original);

            Assert.That(text, Does.Contain("\"type\""));
            Assert.That(text, Does.Contain("\"payload\""));
        }
    }
}
