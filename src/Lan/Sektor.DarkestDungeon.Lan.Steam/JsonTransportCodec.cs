namespace Sektor.DarkestDungeon.Lan.Steam
{
    using Newtonsoft.Json;

    using Sektor.DarkestDungeon.Lan.Contracts.Transport;

    /// <summary>
    /// JSON wire codec built on Newtonsoft.Json. The wire representation carries the message
    /// type and payload as a JSON object; the sender is stamped by the transport at reception.
    /// </summary>
    public sealed class JsonTransportCodec : ITransportCodec
    {
        private const string TypeKey = "type";
        private const string PayloadKey = "payload";

        /// <inheritdoc />
        public string Serialize(TransportMessage message)
        {
            var wire = new System.Collections.Generic.Dictionary<string, string>
            {
                { TypeKey, message.Type },
                { PayloadKey, message.Payload }
            };
            return JsonConvert.SerializeObject(wire);
        }

        /// <inheritdoc />
        public TransportMessage Deserialize(string text)
        {
            var wire = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(text);
            string type;
            string payload;
            wire.TryGetValue(TypeKey, out type);
            wire.TryGetValue(PayloadKey, out payload);
            return new TransportMessage(string.Empty, type, payload);
        }
    }
}
