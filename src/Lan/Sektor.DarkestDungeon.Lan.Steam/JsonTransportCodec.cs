using System.Text;

using Sektor.DarkestDungeon.Lan.Contracts.Transport;

namespace Sektor.DarkestDungeon.Lan.Steam
{
    /// <summary>
    /// Minimal JSON wire codec with no external dependencies. The wire representation carries
    /// the message type and payload as a JSON object with two string fields; the sender is
    /// stamped by the transport at reception. Only the two fields are understood, so the codec
    /// stays safe to run inside Unity 2017.4 (no System.Text.Json / Newtonsoft dependency).
    /// </summary>
    public sealed class JsonTransportCodec : ITransportCodec
    {
        private const string TypeKey = "type";
        private const string PayloadKey = "payload";

        /// <inheritdoc />
        public string Serialize(TransportMessage message)
        {
            return "{\"" + TypeKey + "\":\"" + Escape(message.Type) + "\",\"" + PayloadKey + "\":\"" + Escape(message.Payload) + "\"}";
        }

        /// <inheritdoc />
        public TransportMessage Deserialize(string text)
        {
            return new TransportMessage(string.Empty, ReadStringValue(text, TypeKey), ReadStringValue(text, PayloadKey));
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string ReadStringValue(string text, string key)
        {
            string marker = "\"" + key + "\":\"";
            int start = text.IndexOf(marker, System.StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            int valueStart = start + marker.Length;
            StringBuilder builder = new StringBuilder();
            bool escaped = false;
            for (int i = valueStart; i < text.Length; i++)
            {
                char c = text[i];
                if (escaped)
                {
                    if (c == 'n')
                    {
                        builder.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        builder.Append('\r');
                    }
                    else if (c == 't')
                    {
                        builder.Append('\t');
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                {
                    break;
                }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
