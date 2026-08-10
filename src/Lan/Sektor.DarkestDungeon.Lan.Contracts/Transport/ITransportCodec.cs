namespace Sektor.DarkestDungeon.Lan.Contracts.Transport
{
    /// <summary>
    /// Serializes and deserializes transport messages so that transports only ever move text.
    /// </summary>
    public interface ITransportCodec
    {
        /// <summary>Serializes the message into a wire text representation.</summary>
        string Serialize(TransportMessage message);

        /// <summary>Deserializes the wire text representation back into a message.</summary>
        TransportMessage Deserialize(string text);
    }
}
