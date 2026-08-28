using System;
using System.Linq;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Automatically plays the rival side with random legal skills for local duels against AI.</summary>
    public sealed class AiRivalLink : IDuelRivalLink
    {
        private const int TickMilliseconds = 450;

        private readonly Random random = new Random();
        private readonly DispatcherTimer timer;
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

            string? payload = BuildRandomAction(duel);
            if (payload == null)
                return;

            RivalActionReceived?.Invoke(payload);
        }

        private string? BuildRandomAction(DuelController duel)
        {
            var unit = duel.CurrentUnit!;
            var options = (unit.Character.CurrentCombatSkills ?? Enumerable.Empty<CombatSkill>())
                .Select(skill => new { Skill = skill, Targets = duel.GetAvailableTargets(unit, skill) })
                .Where(x => x.Targets.Count > 0)
                .ToList();
            if (options.Count == 0)
                return null;

            var choice = options[random.Next(options.Count)];
            var target = choice.Targets[random.Next(choice.Targets.Count)];
            return choice.Skill.Id + "|" + target.CombatInfo.CombatId;
        }
    }
}
