using System;
using System.Windows.Threading;

using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Plays the monster side through the campaign monster brain, with the default duel AI as fallback.</summary>
    public sealed class MonsterBrainRivalLink : IDuelRivalLink
    {
        private const int TickMilliseconds = 100;

        private readonly DispatcherTimer timer;
        private readonly FallbackDuelAi fallbackAi = new FallbackDuelAi();
        private DuelController? controller;
        private int lastActedCombatId;

        /// <inheritdoc/>
        public event Action<string>? RivalActionReceived;

        /// <inheritdoc/>
        public event Action<string>? SkillPreviewed;

        /// <inheritdoc/>
        public event Action<int>? TargetPreviewed;

        /// <summary>Initializes a new instance of the <see cref="MonsterBrainRivalLink"/> class.</summary>
        public MonsterBrainRivalLink()
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

            RivalActionReceived?.Invoke(ChooseAction(duel));
        }

        private string ChooseAction(DuelController duel)
        {
            var actor = duel.CurrentUnit;
            if (actor == null)
                return DuelPayload.PassAction();

            if (actor.Character.Brain != null)
            {
                MonsterBrainDecision decision = duel.Solver.UseMonsterBrain(actor);
                if (decision.Decision == BrainDecisionType.Perform &&
                    decision.SelectedSkill != null &&
                    decision.TargetInfo.Targets.Count > 0)
                {
                    return DuelPayload.Skill(
                        decision.SelectedSkill.Id,
                        decision.TargetInfo.Targets[0].CombatInfo.CombatId);
                }
            }
            else
            {
                return fallbackAi.ChooseAction(duel);
            }

            int? moveRank = duel.GetLegalMoveRank(actor);
            return moveRank.HasValue
                ? DuelPayload.MoveAction(moveRank.Value)
                : DuelPayload.PassAction();
        }
    }
}
