using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
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

        /// <summary>Occurs when a popup is shown for a unit (buff/debuff/status/damage).</summary>
        public event Action<ICombatUnit, PopupType, string> PopupShown;

        /// <summary>Gets or sets the callback invoked with a torch delta (wired by the duel controller).</summary>
        public Action<int> TorchDelta { get; set; }

        /// <summary>Gets or sets the callback executed when a hero suffers a heart attack (wired by the duel controller).</summary>
        public Action<ICombatUnit> HeartAttackHandler { get; set; }

        /// <inheritdoc/>
        public void ShowPopup(ICombatUnit target, PopupType type, string value = null)
        {
            Log.Add("[effect] " + target.Character.Name + " " + PopupPhrase(type, value));
            PopupShown?.Invoke(target, type, value);
        }

        /// <summary>Builds the readable verb phrase for a popup type.</summary>
        /// <param name="type">The popup type.</param>
        /// <param name="value">The optional value.</param>
        /// <returns>The phrase.</returns>
        private static string PopupPhrase(PopupType type, string value)
        {
            string withValue = value == null ? string.Empty : " (" + value + ")";
            switch (type)
            {
                case PopupType.Buff:
                    return "gains a buff" + withValue;
                case PopupType.Debuff:
                    return "suffers a debuff" + withValue;
                case PopupType.DebuffResist:
                    return "resists a debuff" + withValue;
                case PopupType.Bleed:
                    return "is bleeding" + withValue;
                case PopupType.BleedResist:
                    return "resists bleeding";
                case PopupType.Poison:
                    return "is blighted" + withValue;
                case PopupType.PoisonResist:
                    return "resists blight";
                case PopupType.Stunned:
                    return "is stunned";
                case PopupType.StunResist:
                    return "resists stun";
                case PopupType.Tagged:
                    return "is marked" + withValue;
                case PopupType.Untagged:
                    return "is unmarked";
                case PopupType.MoveResist:
                    return "resists the move";
                case PopupType.Riposte:
                    return "readies a riposte";
                case PopupType.Guard:
                    return "is now guarded";
                case PopupType.Cured:
                    return "is cured";
                case PopupType.Unstun:
                    return "is unstunned";
                case PopupType.Stress:
                    return "takes " + value + " stress";
                case PopupType.StressHeal:
                    return "heals " + value + " stress";
                case PopupType.Heal:
                    return "heals for " + value;
                case PopupType.CritHeal:
                    return "critically heals for " + value + "!";
                case PopupType.Damage:
                    return "takes " + value + " damage";
                case PopupType.CritDamage:
                    return "takes " + value + " critical damage!";
                case PopupType.ZeroDamage:
                    return "takes no damage";
                case PopupType.DeathsDoor:
                    return "is at death's door!";
                case PopupType.DeathBlow:
                    return "is dealt a death blow!";
                case PopupType.HeartAttack:
                    return "suffers a heart attack!";
                default:
                    return "(" + type + ")" + withValue;
            }
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