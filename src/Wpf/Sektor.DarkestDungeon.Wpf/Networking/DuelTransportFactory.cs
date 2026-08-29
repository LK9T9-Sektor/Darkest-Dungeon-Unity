using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using Sektor.DarkestDungeon.Lan.Steam;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>Creates a duel transport: Steam for real rooms, in-memory for local dev/tests.</summary>
    public static class DuelTransportFactory
    {
        /// <summary>Creates a Steam transport for real multiplayer rooms.</summary>
        /// <returns>The transport.</returns>
        public static ITransport CreateSteamTransport()
        {
            return new SteamTransport(new JsonTransportCodec());
        }

        /// <summary>Creates an in-memory transport for local development.</summary>
        /// <param name="playerId">The local player id.</param>
        /// <returns>The transport.</returns>
        public static InMemoryTransport CreateInMemoryTransport(string playerId)
        {
            return new InMemoryTransport(playerId);
        }
    }
}