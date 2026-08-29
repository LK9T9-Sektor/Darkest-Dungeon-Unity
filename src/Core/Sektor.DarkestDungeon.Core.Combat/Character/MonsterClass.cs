using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Content model of a monster class parsed from a Data\Monsters\*.txt file.</summary>
    public class MonsterClass
    {
        /// <summary>Gets or sets the monster string identifier.</summary>
        public string StringId { get; set; }

        /// <summary>Gets or sets the monster type identifier.</summary>
        public string TypeId { get; set; }

        /// <summary>Gets or sets the monster size.</summary>
        public int Size { get; set; }

        /// <summary>Gets or sets the preferred skill index.</summary>
        public int PreferableSkill { get; set; }

        /// <summary>Gets or sets the monster brain identifier.</summary>
        public string MonsterBrainId { get; set; }

        /// <summary>Gets or sets the number of turns per round.</summary>
        public int InitiativeTurns { get; set; }

        /// <summary>Gets the monster base attributes.</summary>
        public Dictionary<AttributeType, float> Attributes { get; }

        /// <summary>Gets the monster enemy types.</summary>
        public List<MonsterType> EnemyTypes { get; }

        /// <summary>Gets the monster combat skills.</summary>
        public List<CombatSkill> CombatSkills { get; }

        /// <summary>Gets or sets the monster battle modifiers.</summary>
        public BattleModifier Modifiers { get; set; }

        /// <summary>Initializes a new instance of the <see cref="MonsterClass"/> class.</summary>
        public MonsterClass()
        {
            Attributes = new Dictionary<AttributeType, float>();
            EnemyTypes = new List<MonsterType>();
            CombatSkills = new List<CombatSkill>();
            PreferableSkill = -1;
            Size = 1;
            InitiativeTurns = 1;
        }
    }
}