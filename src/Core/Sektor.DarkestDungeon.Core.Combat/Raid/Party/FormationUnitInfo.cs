using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Party
{
    /// <summary>Combat state of a formation unit.</summary>
    public class FormationUnitInfo : IFormationUnitInfo
    {
        /// <inheritdoc/>
        public string LastCombatSkillUsed { get; set; }

        /// <inheritdoc/>
        public int LastCombatSkillTarget { get; set; }

        /// <inheritdoc/>
        public int RoundsAlive { get; set; }

        /// <inheritdoc/>
        public bool MarkedForDeath { get; set; }

        /// <inheritdoc/>
        public bool IsSurprised { get; set; }

        /// <inheritdoc/>
        public bool IsImmobilized { get; set; }

        /// <inheritdoc/>
        public bool IsDead { get; set; }

        /// <inheritdoc/>
        public bool CheckLoot { get; set; }

        /// <inheritdoc/>
        public bool OneShotted { get; set; }

        /// <inheritdoc/>
        public int TotalInitiatives { get; set; }

        /// <inheritdoc/>
        public int CurrentInitiative { get; set; }

        /// <inheritdoc/>
        public double InitiativeRoll { get; set; }

        /// <inheritdoc/>
        public int CombatId { get; set; }

        /// <inheritdoc/>
        public List<string> SkillsUsedInBattle { get; }

        /// <inheritdoc/>
        public List<string> SkillsUsedThisTurn { get; }

        /// <inheritdoc/>
        public List<SkillCooldown> SkillCooldowns { get; }

        /// <inheritdoc/>
        public List<int> BlockedMoveUnitIds { get; }

        /// <inheritdoc/>
        public List<int> BlockedHealUnitIds { get; }

        /// <inheritdoc/>
        public List<int> BlockedBuffUnitIds { get; }

        /// <inheritdoc/>
        public List<string> BlockedItems { get; }

        /// <summary>Initializes a new instance of the <see cref="FormationUnitInfo"/> class.</summary>
        public FormationUnitInfo()
        {
            SkillCooldowns = new List<SkillCooldown>();
            SkillsUsedThisTurn = new List<string>();
            SkillsUsedInBattle = new List<string>();
            BlockedMoveUnitIds = new List<int>();
            BlockedHealUnitIds = new List<int>();
            BlockedBuffUnitIds = new List<int>();
            BlockedItems = new List<string>();
        }

        /// <summary>Prepares the unit for battle with the given combat id.</summary>
        /// <param name="id">The combat id.</param>
        public void PrepareForBattle(int id)
        {
            RoundsAlive = 0;
            LastCombatSkillTarget = -1;
            TotalInitiatives = 1;
            CurrentInitiative = 0;
            CombatId = id;
            IsImmobilized = false;
            IsDead = false;
            MarkedForDeath = false;
            IsSurprised = false;
            CheckLoot = true;
            OneShotted = false;

            SkillsUsedInBattle.Clear();
            SkillsUsedThisTurn.Clear();
            SkillCooldowns.Clear();
            BlockedMoveUnitIds.Clear();
            BlockedHealUnitIds.Clear();
            BlockedBuffUnitIds.Clear();
            BlockedItems.Clear();
        }

        /// <inheritdoc/>
        public void UpdateNextTurn()
        {
            CurrentInitiative++;
            if (CurrentInitiative > TotalInitiatives)
                CurrentInitiative = 1;
        }

        /// <inheritdoc/>
        public void UpdateNextRound()
        {
            RoundsAlive++;
            SkillsUsedThisTurn.Clear();
            CurrentInitiative = 0;
            BlockedMoveUnitIds.Clear();
            BlockedHealUnitIds.Clear();
            BlockedBuffUnitIds.Clear();
            BlockedItems.Clear();
        }
    }
}