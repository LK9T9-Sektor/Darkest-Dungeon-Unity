using Sektor.DarkestDungeon.Core.Combat.Character;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Battle-side service for effect feedback: popups, halos, sounds, summon/control.</summary>
    public interface IBattleEvents
    {
        /// <summary>Shows a popup message over a unit.</summary>
        /// <param name="target">The unit the popup is shown over.</param>
        /// <param name="type">The popup type.</param>
        /// <param name="value">Optional value text.</param>
        void ShowPopup(ICombatUnit target, PopupType type, string value = null);

        /// <summary>Sets a halo animation over a unit.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="haloId">The halo identifier.</param>
        void SetHalo(ICombatUnit unit, string haloId);

        /// <summary>Resets the halo animation over a unit.</summary>
        /// <param name="unit">The unit.</param>
        void ResetHalo(ICombatUnit unit);

        /// <summary>Toggles the defend animation of a unit.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="isDefending">Whether the unit is defending.</param>
        void SetDefendAnimation(ICombatUnit unit, bool isDefending);

        /// <summary>Toggles the combat animation of a unit.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="enabled">Whether animations are enabled.</param>
        void SetCombatAnimation(ICombatUnit unit, bool enabled);

        /// <summary>Refreshes the overlay of a unit.</summary>
        /// <param name="unit">The unit.</param>
        void UpdateOverlay(ICombatUnit unit);

        /// <summary>Refreshes the skill panel for the selected unit.</summary>
        /// <param name="unit">The unit.</param>
        void UpdateSkillPanel(ICombatUnit unit);

        /// <summary>Queues a resolve check for an overstressed hero.</summary>
        /// <param name="unit">The hero unit.</param>
        void AddResolveCheck(ICombatUnit unit);

        /// <summary>Queues a heart attack check for a hero at 200 stress.</summary>
        /// <param name="unit">The hero unit.</param>
        void AddHeartAttackCheck(ICombatUnit unit);

        /// <summary>Plays a sound event.</summary>
        /// <param name="eventPath">The FMOD event path.</param>
        void PlaySound(string eventPath);

        /// <summary>Clears the rank marks of the unit's formation.</summary>
        /// <param name="unit">The unit whose formation ranks are cleared.</param>
        void ClearRankMarks(ICombatUnit unit);

        /// <summary>Marks the rank of a unit for targeting.</summary>
        /// <param name="unit">The unit.</param>
        void MarkRank(ICombatUnit unit);

        /// <summary>Pulls a unit forward by the given amount.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="amount">The pull distance.</param>
        void Pull(ICombatUnit unit, int amount);

        /// <summary>Pushes a unit backward by the given amount.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="amount">The push distance.</param>
        void Push(ICombatUnit unit, int amount);

        /// <summary>Controls (takes over) a target unit for a duration.</summary>
        /// <param name="target">The controlled unit.</param>
        /// <param name="performer">The controlling unit.</param>
        /// <param name="duration">The control duration.</param>
        void ControlUnit(ICombatUnit target, ICombatUnit performer, int duration);

        /// <summary>Summons a monster of the given type at a rank.</summary>
        /// <param name="monsterTypeId">The monster type identifier.</param>
        /// <param name="rank">The target rank.</param>
        /// <param name="rollInitiative">Whether to roll initiative.</param>
        /// <param name="canSpawnLoot">Whether the summon can drop loot.</param>
        void SummonUnit(string monsterTypeId, int rank, bool rollInitiative, bool canSpawnLoot);

        /// <summary>Gets the size of a monster type (for summon space checks).</summary>
        /// <param name="monsterTypeId">The monster type identifier.</param>
        /// <returns>The monster size.</returns>
        int GetMonsterSize(string monsterTypeId);

        /// <summary>Gets the available summon space of the performer's formation.</summary>
        int AvailableSummonSpace { get; }

        /// <summary>Replaces a unit with a full captor unit.</summary>
        /// <param name="fullMonsterTypeId">The full captor monster type identifier.</param>
        /// <param name="unit">The unit being replaced.</param>
        /// <returns>The new full captor unit.</returns>
        ICombatUnit ReplaceUnit(string fullMonsterTypeId, ICombatUnit unit);

        /// <summary>Captures a target unit into a captor unit.</summary>
        /// <param name="target">The captured unit.</param>
        /// <param name="captor">The captor unit.</param>
        /// <param name="removeFromParty">Whether the target is removed from the party.</param>
        void CaptureUnit(ICombatUnit target, ICombatUnit captor, bool removeFromParty);

        /// <summary>Applies the capture effects of an empty captor monster to a target.</summary>
        /// <param name="emptyCaptor">The empty captor unit.</param>
        /// <param name="target">The target unit.</param>
        void ApplyCaptorEffects(ICombatUnit emptyCaptor, ICombatUnit target);

        /// <summary>Sets the capture effect animation on a target.</summary>
        /// <param name="target">The target unit.</param>
        /// <param name="captor">The captor unit.</param>
        void SetCaptureEffect(ICombatUnit target, ICombatUnit captor);

        /// <summary>Decreases the torch by the given amount.</summary>
        /// <param name="amount">The amount to decrease.</param>
        void DecreaseTorch(int amount);

        /// <summary>Increases the torch by the given amount.</summary>
        /// <param name="amount">The amount to increase.</param>
        void IncreaseTorch(int amount);
    }
}