using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Duel-side event sink: records feedback for the UI and no-ops unsupported effects.</summary>
    public class DuelBattleEvents : IBattleEvents
    {
        /// <summary>Gets the recorded event log lines.</summary>
        public List<string> Log { get; } = new List<string>();

        /// <summary>Occurs when the duel state changes (torch, popups).</summary>
        public event Action StateChanged;

        /// <summary>Gets or sets the callback invoked with a torch delta (wired by the duel controller).</summary>
        public Action<int> TorchDelta { get; set; }

        /// <summary>Gets or sets the callback executed when a hero suffers a heart attack (wired by the duel controller).</summary>
        public Action<ICombatUnit> HeartAttackHandler { get; set; }

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
            if (HeartAttackHandler != null)
                HeartAttackHandler(unit);
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
            MoveUnit(unit, -amount);
        }

        /// <inheritdoc/>
        public void Push(ICombatUnit unit, int amount, bool changeUnitOrder = true)
        {
            Log.Add("[push] " + unit.Character.Name + " by " + amount);
            MoveUnit(unit, amount);
        }

        private static void MoveUnit(ICombatUnit unit, int delta)
        {
            if (unit == null || unit.CombatInfo.IsImmobilized)
                return;

            var party = unit.Party;
            if (party == null || party.Units == null || party.Units.Count < 2)
                return;

            int index = party.Units.IndexOf(unit);
            if (index < 0)
                return;

            int target = index + delta;
            if (target < 0)
                target = 0;
            if (target >= party.Units.Count)
                target = party.Units.Count - 1;
            if (target == index)
                return;

            party.Units.RemoveAt(index);
            party.Units.Insert(target, unit);

            for (int i = 0; i < party.Units.Count; i++)
                ((FormationUnit)party.Units[i]).Rank = i + 1;
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
            if (TorchDelta != null)
                TorchDelta(-amount);
        }

        /// <inheritdoc/>
        public void IncreaseTorch(int amount)
        {
            Log.Add("[torch] +" + amount);
            if (TorchDelta != null)
                TorchDelta(amount);
        }
    }
}