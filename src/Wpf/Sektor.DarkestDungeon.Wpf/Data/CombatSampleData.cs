using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Sample combat data built from core types for the battle screen mockup.</summary>
    public static class CombatSampleData
    {
        /// <summary>Builds the sample Crusader combat skill set from core <see cref="CombatSkill"/> instances.</summary>
        /// <returns>The list of combat skills.</returns>
        public static IReadOnlyList<CombatSkill> BuildHeroCombatSkills()
        {
            return new List<CombatSkill>
            {
                new CombatSkill
                {
                    Id = "smite",
                    Category = SkillCategory.Damage,
                    DamageMin = 4,
                    DamageMax = 7,
                    Accuracy = 0.95f,
                    CritMod = 0.05f,
                    LaunchRanks = new FormationSet("1234"),
                    TargetRanks = new FormationSet("1234"),
                },
                new CombatSkill
                {
                    Id = "zealous_accusation",
                    Category = SkillCategory.Damage,
                    DamageMin = 2,
                    DamageMax = 4,
                    Accuracy = 0.85f,
                    CritMod = 0.05f,
                    LaunchRanks = new FormationSet("1234"),
                    TargetRanks = new FormationSet("~1234"),
                },
                new CombatSkill
                {
                    Id = "stunning_blow",
                    Category = SkillCategory.Damage,
                    DamageMin = 3,
                    DamageMax = 6,
                    Accuracy = 0.95f,
                    CritMod = 0.05f,
                    LaunchRanks = new FormationSet("1234"),
                    TargetRanks = new FormationSet("12"),
                },
                new CombatSkill
                {
                    Id = "battle_heal",
                    Category = SkillCategory.Heal,
                    Heal = new HealComponent(3, 5),
                    Accuracy = 0f,
                    LaunchRanks = new FormationSet("1234"),
                    TargetRanks = new FormationSet("@1234"),
                },
            };
        }
    }
}