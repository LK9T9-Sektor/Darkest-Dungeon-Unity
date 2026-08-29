using System;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Automatically plays the rival side for local duels against AI via the core <see cref="DuelAi"/>.</summary>
    public sealed class AiRivalLink : IDuelRivalLink
    {
        private const int TickMilliseconds = 450;

        private readonly DispatcherTimer timer;
        private readonly DuelAi ai = new DuelAi();
        private DuelController? controller;

        /// <inheritdoc/>
        public event Action<string>? RivalActionReceived;

        /// <summary>Initializes a new instance of the <see cref="AiRivalLink"/> class.</summary>
        public AiRivalLink()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TickMilliseconds) };
            timer.Tick += (s, e) => PlayRivalTurn();
        }

        /// <inheritdoc/>
        public void SendLocalAction(string payload)
        {
        }

        /// <inheritdoc/>
        public void Attach(DuelController duel)
        {
            controller = duel;
            timer.Start();
        }

        /// <inheritdoc/>
        public void Detach()
        {
            timer.Stop();
            controller = null;
        }

        /// <inheritdoc/>
        public void Pump()
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Detach();
        }

        private void PlayRivalTurn()
        {
            var duel = controller;
            if (duel == null || !duel.IsStarted || duel.IsFinished || duel.IsLocalTurn || duel.CurrentUnit == null)
                return;

            RivalActionReceived?.Invoke(ai.ChooseAction(duel));
        }
    }
}