using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of character data for combat.</summary>
    public interface ICharacter
    {
        /// <summary>Gets the character's name.</summary>
        string Name { get; }

        /// <summary>Gets the character's class identifier.</summary>
        string Class { get; }

        /// <summary>Gets the character's size.</summary>
        int Size { get; }

        /// <summary>Gets a value indicating whether the character is at death's door.</summary>
        bool AtDeathsDoor { get; }

        /// <summary>Gets a value indicating whether the character is stressed.</summary>
        bool IsStressed { get; }

        /// <summary>Gets a value indicating whether the character is overstressed.</summary>
        bool IsOverstressed { get; }

        /// <summary>Gets a value indicating whether the character is virtued.</summary>
        bool IsVirtued { get; }

        /// <summary>Gets a value indicating whether the character is afflicted.</summary>
        bool IsAfflicted { get; }

        /// <summary>Gets a value indicating whether the character is a monster.</summary>
        bool IsMonster { get; }

        /// <summary>Gets a value indicating whether the character is in a special mode.</summary>
        bool InMode { get; }

        /// <summary>Gets or sets the character's current mode.</summary>
        ICharacterMode CurrentMode { get; set; }

        /// <summary>Gets the character's available modes.</summary>
        List<ICharacterMode> Modes { get; }

        /// <summary>Gets the character's battle modifiers.</summary>
        IBattleModifier BattleModifiers { get; }

        /// <summary>Gets the character's skill art info list.</summary>
        List<SkillArtInfo> SkillArtInfo { get; }

        /// <summary>Gets the character's monster types (for monsters).</summary>
        List<MonsterType> MonsterTypes { get; }

        /// <summary>Gets the character's combat skills (null for non-monsters).</summary>
        List<CombatSkill> CombatSkills { get; }

        /// <summary>Gets the character's monster brain (null for non-monsters).</summary>
        MonsterBrain Brain { get; }

        /// <summary>Gets the character's current health ratio (0-1).</summary>
        float HealthRatio { get; }

        /// <summary>Gets the preferred skill index for monsters (-1 for heroes).</summary>
        int PreferableSkill { get; }

        /// <summary>Gets a value indicating whether the character has zero health.</summary>
        bool HasZeroHealth { get; }

        /// <summary>Gets the empty captor component (null for most characters).</summary>
        IEmptyCaptor EmptyCaptor { get; }

        /// <summary>Gets the controller captor component (null for most monsters).</summary>
        object ControllerCaptor { get; }

        /// <summary>Gets the character's stress meter.</summary>
        IStress Stress { get; }

        /// <summary>Gets a status effect by type.</summary>
        IStatusEffect GetStatusEffect(StatusType type);

        /// <summary>Gets a single attribute value.</summary>
        IAttribute GetSingleAttribute(AttributeType type);

        /// <summary>Heals the character by the given amount.</summary>
        /// <param name="amount">The heal amount.</param>
        /// <param name="isCrit">Whether the heal is a crit.</param>
        /// <returns>The actual health restored.</returns>
        int Heal(float amount, bool isCrit);

        /// <summary>Deals damage equal to a fraction of max health.</summary>
        /// <param name="amount">The fraction in [0, 1].</param>
        void TakeDamagePercent(float amount);

        /// <summary>Adds a buff to the character.</summary>
        /// <param name="buffInfo">The buff instance.</param>
        void AddBuff(BuffInfo buffInfo);

        /// <summary>Reverts the hero's affliction trait (heroes only).</summary>
        void RevertTrait();

        /// <summary>Adds a quirk/disease to a hero.</summary>
        /// <param name="quirk">The quirk to add.</param>
        /// <returns>True if the quirk was added.</returns>
        bool AddQuirk(IQuirk quirk);

        /// <summary>Adds a random disease to a hero.</summary>
        /// <returns>The added disease.</returns>
        IQuirk AddRandomDisease();

        /// <summary>Gets the character's speed.</summary>
        float Speed { get; }

        /// <summary>Gets the character's crit chance.</summary>
        float Crit { get; }

        /// <summary>Gets the character's accuracy.</summary>
        float Accuracy { get; }

        /// <summary>Gets the character's dodge.</summary>
        float Dodge { get; }

        /// <summary>Gets the character's protection.</summary>
        float Protection { get; }

        /// <summary>Gets the character's minimum damage.</summary>
        float MinDamage { get; }

        /// <summary>Gets the character's maximum damage.</summary>
        float MaxDamage { get; }

        /// <summary>Gets the character's damage modifier.</summary>
        float DamageMod { get; }

        /// <summary>Gets the riposte skill (null for most characters).</summary>
        CombatSkill RiposteSkill { get; }

        /// <summary>Gets the hero's current combat skills (heroes only).</summary>
        List<CombatSkill> CurrentCombatSkills { get; }

        /// <summary>Gets a value indicating whether the hero's class is religious.</summary>
        bool IsReligious { get; }

        /// <summary>Deals direct damage to the character.</summary>
        /// <param name="damageAmount">The damage amount.</param>
        /// <returns>The actual damage taken.</returns>
        int TakeDamage(float damageAmount);

        /// <summary>Removes conditional buffs from the character.</summary>
        void RemoveConditionalBuffs();
    }
}