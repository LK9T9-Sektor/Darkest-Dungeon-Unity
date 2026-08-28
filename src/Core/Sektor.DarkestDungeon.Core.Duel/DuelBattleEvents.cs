using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Duel-side event sink: records feedback for the UI and no-ops unsupported effects.</summary>
    public class DuelBattleEvents : IBattleEvents
    {
        /// <summary>Gets the recorded event log lines.</summary>
        public List<string> Log { get; } = new List<string>();

        /// <summary>Occurs when the duel state changes (torch, popups).</summary>
        public event Action StateChanged;

        /// <inheritdoc/>
        public void ShowPopup(ICombatUnit target, PopupType type, string value = null)
        {
            Log.Add("[popup] " + type + (value == null ? "" : " " + value) + " on " + target.Character.Name);
        }

        /// <inheritdoc/>
        public void SetHalo(ICombatUnit unit, string haloId)
        {
            Log.Add("[halo] " + haloId + " on " + unit.Character.Name);
        }

        /// <inheritdoc/>
        public void ResetHalo(ICombatUnit unit)
        {
        }

        /// <inheritdoc/>
        public void SetDefendAnimation(ICombatUnit unit, bool isDefending)
        {
        }

        /// <inheritdoc/>
        public void SetCombatAnimation(ICombatUnit unit, bool enabled)
        {
        }

        /// <inheritdoc/>
        public void UpdateOverlay(ICombatUnit unit)
        {
        }

        /// <inheritdoc/>
        public void UpdateSkillPanel(ICombatUnit unit)
        {
        }

        /// <inheritdoc/>
        public void AddResolveCheck(ICombatUnit unit)
        {
            Log.Add("[resolve] check for " + unit.Character.Name);
        }

        /// <inheritdoc/>
        public void AddHeartAttackCheck(ICombatUnit unit)
        {
            Log.Add("[heart-attack] check for " + unit.Character.Name);
        }

        /// <inheritdoc/>
        public void PlaySound(string eventPath)
        {
            Log.Add("[sound] " + eventPath);
        }

        /// <inheritdoc/>
        public void ClearRankMarks(ICombatUnit unit)
        {
        }

        /// <inheritdoc/>
        public void MarkRank(ICombatUnit unit)
        {
        }

        /// <inheritdoc/>
        public void Pull(ICombatUnit unit, int amount, bool changeUnitOrder = true)
        {
            Log.Add("[pull] " + unit.Character.Name + " by " + amount);
        }

        /// <inheritdoc/>
        public void Push(ICombatUnit unit, int amount, bool changeUnitOrder = true)
        {
            Log.Add("[push] " + unit.Character.Name + " by " + amount);
        }

        /// <inheritdoc/>
        public void ControlUnit(ICombatUnit target, ICombatUnit performer, int duration)
        {
            Log.Add("[control] " + target.Character.Name + " for " + duration);
        }

        /// <inheritdoc/>
        public void SummonUnit(string monsterTypeId, int rank, bool rollInitiative, bool canSpawnLoot)
        {
            Log.Add("[summon] " + monsterTypeId + " at rank " + rank);
        }

        /// <inheritdoc/>
        public int GetMonsterSize(string monsterTypeId)
        {
            return 1;
        }

        /// <inheritdoc/>
        public int AvailableSummonSpace { get { return 0; } }

        /// <inheritdoc/>
        public ICombatUnit ReplaceUnit(string fullMonsterTypeId, ICombatUnit unit)
        {
            return unit;
        }

        /// <inheritdoc/>
        public void CaptureUnit(ICombatUnit target, ICombatUnit captor, bool removeFromParty)
        {
            Log.Add("[capture] " + target.Character.Name + " by " + captor.Character.Name);
        }

        /// <inheritdoc/>
        public void ApplyCaptorEffects(ICombatUnit emptyCaptor, ICombatUnit target)
        {
        }

        /// <inheritdoc/>
        public void SetCaptureEffect(ICombatUnit target, ICombatUnit captor)
        {
        }

        /// <inheritdoc/>
        public void DecreaseTorch(int amount)
        {
            Log.Add("[torch] -" + amount);
        }

        /// <inheritdoc/>
        public void IncreaseTorch(int amount)
        {
            Log.Add("[torch] +" + amount);
        }
    }
}