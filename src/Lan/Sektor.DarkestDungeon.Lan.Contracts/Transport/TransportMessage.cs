namespace Sektor.DarkestDungeon.Lan.Contracts.Transport
{
    /// <summary>
    /// A single logical message exchanged between participants of a session.
    /// The transport stamps the sender identifier and delivers the message reliably and in order.
    /// </summary>
    public sealed class TransportMessage
    {
        private readonly string _senderId;
        private readonly string _type;
        private readonly string _payload;

        /// <summary>Creates a message with the given sender, type and payload.</summary>
        public TransportMessage(string senderId, string type, string payload)
        {
            _senderId = senderId;
            _type = type;
            _payload = payload;
        }

        /// <summary>Gets the opaque identifier of the sending player.</summary>
        public string SenderId
        {
            get { return _senderId; }
        }

        /// <summary>Gets the message type identifier; string ids are validated at content load time.</summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>Gets the serialized payload; the JSON codec serializes it as text.</summary>
        public string Payload
        {
            get { return _payload; }
        }
    }
}
