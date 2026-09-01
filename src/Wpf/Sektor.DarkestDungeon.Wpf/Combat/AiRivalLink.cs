using System;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Automatically plays the rival side for local duels against AI via the core <see cref="DuelAi"/>,
    /// paced on the UI side: the AI "selects" a skill, then a target, then waits before acting.</summary>
    public sealed class AiRivalLink : IDuelRivalLink
    {
        private const int TickMilliseconds = 400;
        private const int PlanningTicks = 1;
        private const int SkillRevealTicks = 2;
        private const int TargetRevealTicks = 3;
        private const int ExecuteDelayTicks = 5;

        private enum Phase
        {
            Idle,
            Planning,
            SkillReveal,
            TargetReveal,
            Delay,
        }

        private readonly DispatcherTimer timer;
        private readonly DuelAi ai = new DuelAi();
        private DuelController? controller;
        private Phase phase = Phase.Idle;
        private int phaseTicks;
        private string? pendingPayload;
        private string? pendingSkillId;
        private int pendingTargetId;

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
            phase = Phase.Idle;
            phaseTicks = 0;
            timer.Start();
        }

        /// <inheritdoc/>
        public void Detach()
        {
            timer.Stop();
            controller = null;
            pendingPayload = null;
            phase = Phase.Idle;
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
            {
                phase = Phase.Idle;
                pendingPayload = null;
                return;
            }

            switch (phase)
            {
                case Phase.Idle:
                    pendingPayload = ai.ChooseAction(duel);
                    ParsePayload(pendingPayload);
                    phase = Phase.Planning;
                    phaseTicks = 0;
                    break;

                case Phase.Planning:
                    if (++phaseTicks >= PlanningTicks)
                    {
                        SkillPreviewed?.Invoke(pendingSkillId ?? string.Empty);
                        phase = Phase.SkillReveal;
                        phaseTicks = 0;
                    }
                    break;

                case Phase.SkillReveal:
                    if (++phaseTicks >= SkillRevealTicks)
                    {
                        TargetPreviewed?.Invoke(pendingTargetId);
                        phase = Phase.TargetReveal;
                        phaseTicks = 0;
                    }
                    break;

                case Phase.TargetReveal:
                    if (++phaseTicks >= TargetRevealTicks)
                    {
                        phase = Phase.Delay;
                        phaseTicks = 0;
                    }
                    break;

                case Phase.Delay:
                    if (++phaseTicks >= ExecuteDelayTicks)
                    {
                        RivalActionReceived?.Invoke(pendingPayload ?? DuelPayload.PassAction());
                        pendingPayload = null;
                        phase = Phase.Idle;
                    }
                    break;
            }
        }

        private void ParsePayload(string payload)
        {
            var parts = (payload ?? string.Empty).Split('|');
            pendingSkillId = parts.Length > 0 ? parts[0] : DuelPayload.Pass;
            int.TryParse(parts.Length > 1 ? parts[1] : "0", out pendingTargetId);
        }
    }
}