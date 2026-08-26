using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Abstract base for bonus initiative desires.</summary>
    public abstract class BonusInitiativeDesire
    {
        /// <summary>Gets the combat skill override identifier.</summary>
        public string CombatSkillOverride { get; private set; }

        /// <summary>Gets a value indicating whether the desire applies at round start.</summary>
        public bool IsRoundStart { get; private set; }

        /// <summary>Gets a value indicating whether the desire applies during round progress.</summary>
        public bool IsRoundInProgress { get; private set; }

        /// <summary>Gets a value indicating whether the desire applies at round finish.</summary>
        public bool IsRoundFinish { get; private set; }

        /// <summary>Gets a value indicating whether the desire applies at post turn.</summary>
        public bool IsPostTurn { get; private set; }

        /// <summary>Checks whether the bonus initiative applies for the performer.</summary>
        /// <param name="performer">The combat unit.</param>
        /// <param name="battleContext">The battle context.</param>
        /// <returns>True if the bonus initiative applies.</returns>
        public abstract bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext);

        /// <summary>Populates settings from a data set dictionary.</summary>
        /// <param name="dataSet">The data set to process.</param>
        protected virtual void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
                ProcessBaseDataToken(token);
        }

        /// <summary>Processes a single key-value token from the data set.</summary>
        /// <param name="token">The key-value pair to process.</param>
        protected void ProcessBaseDataToken(KeyValuePair<string, object> token)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)token.Value;
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)token.Value;
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)token.Value;
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)token.Value;
                    break;
                case "is_pre_turn":
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)token.Value;
                    break;
            }
        }
    }
}
