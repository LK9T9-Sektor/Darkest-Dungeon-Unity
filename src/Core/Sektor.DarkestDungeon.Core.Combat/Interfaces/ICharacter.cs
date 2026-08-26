using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
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

        /// <summary>Gets the character's current mode.</summary>
        ICharacterMode Mode { get; }

        /// <summary>Gets the character's battle modifiers.</summary>
        IBattleModifier BattleModifiers { get; }

        /// <summary>Gets the character's skill art info list.</summary>
        List<SkillArtInfo> SkillArtInfo { get; }

        /// <summary>Gets the character's monster types (for monsters).</summary>
        List<MonsterType> MonsterTypes { get; }

        /// <summary>Gets the character's combat skills (null for non-monsters).</summary>
        List<CombatSkill> CombatSkills { get; }

        /// <summary>Gets the character's monster brain (null for non-monsters).</summary>
        AI.MonsterBrain Brain { get; }

        /// <summary>Gets the character's current health ratio (0-1).</summary>
        float HealthRatio { get; }

        /// <summary>Gets the preferred skill index for monsters (-1 for heroes).</summary>
        int PreferableSkill { get; }

        /// <summary>Gets a value indicating whether the character has zero health.</summary>
        bool HasZeroHealth { get; }

        /// <summary>Gets the empty captor component (null for most characters).</summary>
        object EmptyCaptor { get; }

        /// <summary>Gets a status effect by type.</summary>
        IStatusEffect GetStatusEffect(StatusType type);

        /// <summary>Gets a single attribute value.</summary>
        IAttribute GetSingleAttribute(AttributeType type);

        /// <summary>Gets the character's speed.</summary>
        float Speed { get; }

        /// <summary>Gets the character's crit chance.</summary>
        float Crit { get; }

        /// <summary>Gets the character's accuracy.</summary>
        float Accuracy { get; }
    }
}
