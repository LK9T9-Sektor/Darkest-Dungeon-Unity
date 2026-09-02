using System;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Automatically plays the rival side for local duels against AI via the core <see cref="DuelAi"/>.
    /// Acting is immediate: the duel view model owns the reveal/swap timing, so this link just emits the
    /// chosen action once per rival turn instead of pacing it itself.</summary>
    public sealed class AiRivalLink : IDuelRivalLink
    {
        private const int TickMilliseconds = 100;

        private readonly DispatcherTimer timer;
        private readonly DuelAi ai = new DuelAi();
        private DuelController? controller;
        private int lastActedCombatId;

        /// <inheritdoc/>
        public event Action<string>? RivalActionReceived;

        /// <inheritdoc/>
        public event Action<string>? SkillPreviewed;

        /// <inheritdoc/>
        public event Action<int>? TargetPreviewed;

        /// <summary>Initializes a new instance of the <see cref="AiRivalLink"/> class.</summary>
        public AiRivalLink()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TickMilliseconds) };
            timer.Tick += (s, e) => Tick();
        }

        /// <inheritdoc/>
        public void SendLocalAction(string payload)
        {
        }

        /// <inheritdoc/>
        public void Attach(DuelController duel)
        {
            controller = duel;
            lastActedCombatId = 0;
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

        private void Tick()
        {
            var duel = controller;
            if (duel == null || !duel.IsStarted || duel.IsFinished || duel.IsLocalTurn || duel.CurrentUnit == null)
                return;

            int id = duel.CurrentUnit.CombatInfo.CombatId;
            if (id == lastActedCombatId)
                return;
            lastActedCombatId = id;

            string payload = ai.ChooseAction(duel) ?? DuelPayload.PassAction();
            RivalActionReceived?.Invoke(payload);
        }
    }
}