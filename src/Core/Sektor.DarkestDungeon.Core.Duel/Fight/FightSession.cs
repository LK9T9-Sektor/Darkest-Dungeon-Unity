using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel.Fight
{
    /// <summary>
    /// Automated campaign fight runner: builds a <see cref="DuelController"/> from the given content,
    /// starts it and drives every unit autonomously (heroes through a default brain, monsters through
    /// their campaign brains) until the battle ends.
    /// </summary>
    public sealed class FightSession
    {
        private readonly IDuelContent content;
        private readonly int seed;
        private readonly DuelAi rivalAi = new DuelAi();
        private bool started;

        /// <summary>Initializes a new instance of the <see cref="FightSession"/> class.</summary>
        /// <param name="content">The campaign content source.</param>
        /// <param name="seed">The deterministic session seed.</param>
        public FightSession(IDuelContent content, int seed)
        {
            this.content = content;
            this.seed = seed;
        }

        /// <summary>Gets the running duel controller (null until <see cref="Start"/> is called).</summary>
        public DuelController Duel { get; private set; }

        /// <summary>Gets a value indicating whether the fight has been started.</summary>
        public bool IsStarted { get { return started && Duel != null; } }

        /// <summary>Gets a value indicating whether the fight has finished.</summary>
        public bool IsFinished { get { return Duel != null && Duel.IsFinished; } }

        /// <summary>Starts the fight between the given sides and rolls the first round.</summary>
        /// <param name="playerSide">The player side unit specifications.</param>
        /// <param name="aiSide">The AI side unit specifications.</param>
        public void Start(IReadOnlyList<FightUnitSpec> playerSide, IReadOnlyList<FightUnitSpec> aiSide)
        {
            Duel = new DuelController(content);
            Duel.StartFight(playerSide, aiSide, seed);
            Duel.StartBattle();
            started = true;
        }

        /// <summary>Advances the fight by one acting unit's turn.</summary>
        /// <returns>True while the fight is still running, false once it has finished.</returns>
        public bool Tick()
        {
            if (!IsStarted || IsFinished)
                return false;

            if (Duel.Phase != DuelPhase.WaitingForHostAction && Duel.Phase != DuelPhase.WaitingForClientAction)
                return false;

            string payload = DecideAction();
            if (string.IsNullOrEmpty(payload))
                payload = DuelPayload.PassAction();

            if (Duel.IsLocalTurn)
            {
                if (!TryExecuteLocalSkill(payload))
                    Duel.ExecuteLocalPass();
            }
            else
            {
                Duel.ApplyRemoteSkill(payload);
            }

            return !IsFinished;
        }

        /// <summary>Runs the fight until it finishes.</summary>
        public void RunToCompletion()
        {
            while (Tick())
            {
            }
        }

        private string DecideAction()
        {
            ICombatUnit unit = Duel.CurrentUnit;
            if (unit == null || Duel.Context == null)
                return null;

            if (unit.Character.IsMonster && unit.Character.Brain != null)
            {
                MonsterBrainDecision decision = Duel.Solver.UseMonsterBrain(unit);
                if (decision.Decision == BrainDecisionType.Perform &&
                    decision.SelectedSkill != null &&
                    decision.TargetInfo.Targets.Count > 0)
                {
                    return DuelPayload.Skill(
                        decision.SelectedSkill.Id,
                        decision.TargetInfo.Targets[0].CombatInfo.CombatId);
                }

                return DuelPayload.PassAction();
            }

            return rivalAi.ChooseAction(Duel);
        }

        private bool TryExecuteLocalSkill(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || parts[0] == DuelPayload.Pass || parts[0] == DuelPayload.Move)
                return false;

            int targetId;
            if (!int.TryParse(parts[1], out targetId))
                return false;

            return Duel.ExecuteLocalSkill(parts[0], targetId) != null;
        }
    }
}