using System;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>Routes duel actions over an active transport session.</summary>
    public sealed class NetworkRivalLink : IDuelRivalLink
    {
        private readonly DuelSessionManager session;

        /// <inheritdoc/>
        public event Action<string>? RivalActionReceived;

        /// <inheritdoc/>
        public event Action<string>? SkillPreviewed;

        /// <inheritdoc/>
        public event Action<int>? TargetPreviewed;

        /// <summary>Initializes a new instance of the <see cref="NetworkRivalLink"/> class.</summary>
        /// <param name="session">The session manager owning the transport.</param>
        public NetworkRivalLink(DuelSessionManager session)
        {
            this.session = session;
            session.RivalInputReceived += OnRivalInputReceived;
        }

        /// <inheritdoc/>
        public void SendLocalAction(string payload)
        {
            session.SendInput(DuelWire.Rpc(DuelWire.HeroSkill), payload);
        }

        /// <inheritdoc/>
        public void Attach(DuelController controller)
        {
        }

        /// <inheritdoc/>
        public void Detach()
        {
        }

        /// <inheritdoc/>
        public void Pump()
        {
            session.Pump();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            session.RivalInputReceived -= OnRivalInputReceived;
        }

        private void OnRivalInputReceived(string method, string payload)
        {
            if (method == DuelWire.HeroSkill)
                RivalActionReceived?.Invoke(payload);
        }
    }
}
