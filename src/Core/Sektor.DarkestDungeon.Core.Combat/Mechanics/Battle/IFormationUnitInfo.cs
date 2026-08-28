using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Abstraction of combat state information for a unit.</summary>
    public interface IFormationUnitInfo
    {
        /// <summary>Gets or sets the last combat skill used.</summary>
        string LastCombatSkillUsed { get; set; }

        /// <summary>Gets or sets the last combat skill target combat ID.</summary>
        int LastCombatSkillTarget { get; set; }

        /// <summary>Gets or sets the number of rounds alive.</summary>
        int RoundsAlive { get; set; }

        /// <summary>Gets or sets a value indicating whether the unit is marked for death.</summary>
        bool MarkedForDeath { get; set; }

        /// <summary>Gets or sets a value indicating whether the unit is surprised.</summary>
        bool IsSurprised { get; set; }

        /// <summary>Gets or sets a value indicating whether the unit is immobilized.</summary>
        bool IsImmobilized { get; set; }

        /// <summary>Gets or sets a value indicating whether the unit is dead.</summary>
        bool IsDead { get; set; }

        /// <summary>Gets or sets a value indicating whether to check loot on death.</summary>
        bool CheckLoot { get; set; }

        /// <summary>Gets or sets a value indicating whether the unit was one-shotted.</summary>
        bool OneShotted { get; set; }

        /// <summary>Gets or sets the total initiatives.</summary>
        int TotalInitiatives { get; set; }

        /// <summary>Gets or sets the current initiative.</summary>
        int CurrentInitiative { get; set; }

        /// <summary>Gets or sets the round's rolled initiative (speed + roll) used to order turns.</summary>
        double InitiativeRoll { get; set; }

        /// <summary>Gets or sets the unique combat identifier.</summary>
        int CombatId { get; set; }

        /// <summary>Gets the list of skills used in this battle.</summary>
        List<string> SkillsUsedInBattle { get; }

        /// <summary>Gets the list of skills used this turn.</summary>
        List<string> SkillsUsedThisTurn { get; }

        /// <summary>Gets the list of skill cooldowns.</summary>
        List<Mechanics.Battle.SkillCooldown> SkillCooldowns { get; }

        /// <summary>Gets the list of blocked move unit IDs.</summary>
        List<int> BlockedMoveUnitIds { get; }

        /// <summary>Gets the list of blocked heal unit IDs.</summary>
        List<int> BlockedHealUnitIds { get; }

        /// <summary>Gets the list of blocked buff unit IDs.</summary>
        List<int> BlockedBuffUnitIds { get; }

        /// <summary>Gets the list of blocked item IDs.</summary>
        List<string> BlockedItems { get; }

        /// <summary>Updates the next turn initiative.</summary>
        void UpdateNextTurn();

        /// <summary>Prepares the unit for the next round.</summary>
        void UpdateNextRound();
    }
}
