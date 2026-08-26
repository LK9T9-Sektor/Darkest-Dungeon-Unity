using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Sample hero class definitions shared by both duel clients (same data on both sides).</summary>
    public static class DuelClasses
    {
        private static readonly Dictionary<string, HeroClass> Classes = BuildClasses();

        /// <summary>Gets a hero class by its string id.</summary>
        /// <param name="classId">The class id.</param>
        /// <returns>The hero class, or null if unknown.</returns>
        public static HeroClass? Get(string classId)
        {
            return Classes.TryGetValue(classId, out var heroClass) ? heroClass : null;
        }

        private static Dictionary<string, HeroClass> BuildClasses()
        {
            var crusader = new HeroClass
            {
                StringId = "crusader",
                Attributes = new Dictionary<AttributeType, float>
                {
                    { AttributeType.HitPoints, 40 },
                    { AttributeType.SpeedRating, 0 },
                    { AttributeType.AttackRating, 0 },
                    { AttributeType.CritChance, 0 },
                    { AttributeType.DamageLow, 5 },
                    { AttributeType.DamageHigh, 8 },
                    { AttributeType.DefenseRating, 0 },
                    { AttributeType.ProtectionRating, 0 },
                },
                CombatSkills = new List<CombatSkill>
                {
                    Skill("smite", 4, 7, 0.95f, 0.05f, "1234", "1234"),
                    Skill("zealous_accusation", 2, 4, 0.85f, 0.05f, "1234", "~1234"),
                    Skill("stunning_blow", 3, 6, 0.95f, 0.05f, "1234", "12"),
                },
            };
            crusader.Resistances = BaseResistances();

            var highwayman = new HeroClass
            {
                StringId = "highwayman",
                Attributes = new Dictionary<AttributeType, float>
                {
                    { AttributeType.HitPoints, 32 },
                    { AttributeType.SpeedRating, 2 },
                    { AttributeType.AttackRating, 0 },
                    { AttributeType.CritChance, 0.05f },
                    { AttributeType.DamageLow, 4 },
                    { AttributeType.DamageHigh, 9 },
                    { AttributeType.DefenseRating, 0 },
                    { AttributeType.ProtectionRating, 0 },
                },
                CombatSkills = new List<CombatSkill>
                {
                    Skill("point_blank_shot", 5, 9, 0.9f, 0.07f, "12", "1234"),
                    Skill("wicked_slice", 4, 7, 0.95f, 0.05f, "12", "1234"),
                },
            };
            highwayman.Resistances = BaseResistances();

            var plagueDoctor = new HeroClass
            {
                StringId = "plague_doctor",
                Attributes = new Dictionary<AttributeType, float>
                {
                    { AttributeType.HitPoints, 26 },
                    { AttributeType.SpeedRating, 3 },
                    { AttributeType.AttackRating, 0 },
                    { AttributeType.CritChance, 0 },
                    { AttributeType.DamageLow, 3 },
                    { AttributeType.DamageHigh, 5 },
                    { AttributeType.DefenseRating, 0 },
                    { AttributeType.ProtectionRating, 0 },
                },
                CombatSkills = new List<CombatSkill>
                {
                    Skill("noxious_blast", 2, 4, 0.95f, 0.05f, "1234", "1234"),
                    Skill("plague_grenade", 2, 3, 0.9f, 0.05f, "1234", "~1234"),
                },
            };
            plagueDoctor.Resistances = BaseResistances();

            var vestal = new HeroClass
            {
                StringId = "vestal",
                Attributes = new Dictionary<AttributeType, float>
                {
                    { AttributeType.HitPoints, 34 },
                    { AttributeType.SpeedRating, 0 },
                    { AttributeType.AttackRating, 0 },
                    { AttributeType.CritChance, 0 },
                    { AttributeType.DamageLow, 3 },
                    { AttributeType.DamageHigh, 5 },
                    { AttributeType.DefenseRating, 0 },
                    { AttributeType.ProtectionRating, 0 },
                },
                CombatSkills = new List<CombatSkill>
                {
                    new CombatSkill
                    {
                        Id = "divine_comfort",
                        Category = SkillCategory.Heal,
                        Heal = new HealComponent(3, 5),
                        LaunchRanks = new FormationSet("1234"),
                        TargetRanks = new FormationSet("@~1234"),
                    },
                },
            };
            vestal.Resistances = BaseResistances();

            return new Dictionary<string, HeroClass>
            {
                { "crusader", crusader },
                { "highwayman", highwayman },
                { "plague_doctor", plagueDoctor },
                { "vestal", vestal },
            };
        }

        private static Dictionary<AttributeType, float> BaseResistances()
        {
            return new Dictionary<AttributeType, float>
            {
                { AttributeType.Stun, 0.4f },
                { AttributeType.Poison, 0.4f },
                { AttributeType.Bleed, 0.4f },
                { AttributeType.Disease, 0.4f },
                { AttributeType.Move, 0.4f },
                { AttributeType.Debuff, 0.4f },
                { AttributeType.DeathBlow, 0.4f },
                { AttributeType.Trap, 0.4f },
            };
        }

        private static CombatSkill Skill(string id, float min, float max, float accuracy, float crit, string launch, string target)
        {
            return new CombatSkill
            {
                Id = id,
                Category = SkillCategory.Damage,
                DamageMin = min,
                DamageMax = max,
                Accuracy = accuracy,
                CritMod = crit,
                IsCritValid = true,
                CanMiss = null,
                LaunchRanks = new FormationSet(launch),
                TargetRanks = new FormationSet(target),
            };
        }
    }
}