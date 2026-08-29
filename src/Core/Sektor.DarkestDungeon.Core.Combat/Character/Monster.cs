using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Combat model of a monster built from a <see cref="MonsterClass"/>.</summary>
    public class Monster : Character
    {
        private readonly MonsterClass _monsterClass;
        private MonsterBrain _brain;

        /// <summary>Initializes a new instance of the <see cref="Monster"/> class.</summary>
        /// <param name="monsterClass">The monster content model.</param>
        public Monster(MonsterClass monsterClass)
        {
            _monsterClass = monsterClass;

            this[AttributeType.HitPoints, true].RawValue = monsterClass.Attributes[AttributeType.HitPoints];
            this[AttributeType.HitPoints, true].CurrentValue = this[AttributeType.HitPoints, true].ModifiedValue;

            if (monsterClass.Attributes.ContainsKey(AttributeType.DefenseRating))
                this[AttributeType.DefenseRating].RawValue = monsterClass.Attributes[AttributeType.DefenseRating];
            if (monsterClass.Attributes.ContainsKey(AttributeType.ProtectionRating))
                this[AttributeType.ProtectionRating].RawValue = monsterClass.Attributes[AttributeType.ProtectionRating];
            if (monsterClass.Attributes.ContainsKey(AttributeType.SpeedRating))
                this[AttributeType.SpeedRating].RawValue = monsterClass.Attributes[AttributeType.SpeedRating];

            this[AttributeType.DamageHigh].RawValue = 1f;

            foreach (var resistance in MonsterResistances)
            {
                float value = monsterClass.Attributes.ContainsKey(resistance)
                    ? monsterClass.Attributes[resistance]
                    : 0f;
                AddSingleAttribute(resistance, new SingleAttribute(value));
            }
        }

        /// <summary>Gets the monster resistance attribute types.</summary>
        public static readonly AttributeType[] MonsterResistances = new AttributeType[]
        {
            AttributeType.Stun, AttributeType.Poison, AttributeType.Bleed, AttributeType.Debuff, AttributeType.Move,
        };

        /// <summary>Assigns the monster brain used for combat decisions.</summary>
        /// <param name="brain">The monster brain, or null to act via the preferred skill.</param>
        public void AssignBrain(MonsterBrain brain)
        {
            _brain = brain;
        }

        /// <inheritdoc/>
        public override string Name { get { return _monsterClass.StringId; } }

        /// <inheritdoc/>
        public override string Class { get { return _monsterClass.TypeId; } }

        /// <inheritdoc/>
        public override int Size { get { return _monsterClass.Size; } }

        /// <inheritdoc/>
        public override int NumberOfTurns { get { return Math.Max(1, _monsterClass.InitiativeTurns); } }

        /// <inheritdoc/>
        public override bool IsMonster { get { return true; } }

        /// <inheritdoc/>
        public override IBattleModifier BattleModifiers { get { return _monsterClass.Modifiers; } }

        /// <inheritdoc/>
        public override List<MonsterType> MonsterTypes { get { return _monsterClass.EnemyTypes; } }

        /// <inheritdoc/>
        public override List<CombatSkill> CombatSkills { get { return _monsterClass.CombatSkills; } }

        /// <inheritdoc/>
        public override List<CombatSkill> CurrentCombatSkills { get { return _monsterClass.CombatSkills; } }

        /// <inheritdoc/>
        public override MonsterBrain Brain { get { return _brain; } }

        /// <inheritdoc/>
        public override int PreferableSkill { get { return _monsterClass.PreferableSkill; } }
    }
}