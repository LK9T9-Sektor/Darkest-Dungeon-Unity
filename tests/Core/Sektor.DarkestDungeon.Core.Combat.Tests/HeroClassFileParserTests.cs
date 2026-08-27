namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Character;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

    [TestFixture]
    public class HeroClassFileParserTests
    {
        private const string Sample = @"
name: test_knight

art:
combat_skill: .id ""smite"" .icon ""one""
.end

info:
resistances: .stun 40% .trap 10%
weapon: .name ""w_0"" .atk 0% .dmg 6 12 .crit 5% .spd 1
armour: .name ""a_0"" .def 5% .prot 0 .hp 33 .spd 2
combat_skill: .id ""smite"" .level 1 .type ""melee"" .atk 90% .dmg -30% .launch 21 .target 12
combat_skill: .id ""smite"" .level 0 .type ""melee"" .atk 85% .dmg -40% .crit 1% .launch 21 .target 12 .is_crit_valid True
combat_skill: .id ""smite"" .level 4 .type ""melee"" .atk 105% .dmg -20% .launch 21 .target 12
combat_skill: .id ""mend"" .level 0 .heal 2 3 .launch 12 .target @123
skill_selection: .can_select_combat_skills true .number_of_selected_combat_skills_max 4
tag: .id ""religious""
id_index: .index 7
.end
";

        [Test]
        public void Parse_LoadsBaseRankStatsSkillsResistancesAndTags()
        {
            var heroClass = HeroClassFileParser.Parse(Sample);

            Assert.That(heroClass, Is.Not.Null);
            Assert.That(heroClass.StringId, Is.EqualTo("test_knight"));
            Assert.That(heroClass.IndexId, Is.EqualTo(7));
            Assert.That(heroClass.IsReligious, Is.True);
            Assert.That(heroClass.CanSelectCombatSkills, Is.True);
            Assert.That(heroClass.NumberOfSelectedCombatSkills, Is.EqualTo(4));

            Assert.That(heroClass.Attributes[AttributeType.HitPoints], Is.EqualTo(33f));
            Assert.That(heroClass.Attributes[AttributeType.DamageLow], Is.EqualTo(6f));
            Assert.That(heroClass.Attributes[AttributeType.DamageHigh], Is.EqualTo(12f));
            Assert.That(heroClass.Attributes[AttributeType.CritChance], Is.EqualTo(0.05f).Within(0.0005f));
            Assert.That(heroClass.Attributes[AttributeType.SpeedRating], Is.EqualTo(3f));
            Assert.That(heroClass.Attributes[AttributeType.ProtectionRating], Is.EqualTo(0f));
            Assert.That(heroClass.Attributes[AttributeType.DefenseRating], Is.EqualTo(0.05f).Within(0.0005f));

            Assert.That(heroClass.Resistances[AttributeType.Stun], Is.EqualTo(0.4f).Within(0.0005f));
            Assert.That(heroClass.Resistances[AttributeType.Trap], Is.EqualTo(0.1f).Within(0.0005f));

            // Only level-0 skills are loaded; higher levels of the same id are ignored.
            Assert.That(heroClass.CombatSkills.Count, Is.EqualTo(2));

            var smite = heroClass.CombatSkills.Single(skill => skill.Id == "smite");
            Assert.That(smite.Category, Is.EqualTo(SkillCategory.Damage));
            Assert.That(smite.Accuracy, Is.EqualTo(0.85f).Within(0.0005f));
            Assert.That(smite.DamageMod, Is.EqualTo(-0.4f).Within(0.0005f));
            Assert.That(smite.CritMod, Is.EqualTo(0.01f).Within(0.0005f));
            Assert.That(smite.IsCritValid, Is.True);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, smite.LaunchRanks.Ranks);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, smite.TargetRanks.Ranks);

            var mend = heroClass.CombatSkills.Single(skill => skill.Id == "mend");
            Assert.That(mend.Category, Is.EqualTo(SkillCategory.Heal));
            Assert.That(mend.Heal.MinAmount, Is.EqualTo(2));
            Assert.That(mend.Heal.MaxAmount, Is.EqualTo(3));
        }

        [Test]
        public void Parse_AllHeroesFromUnityContent_LoadsFullRoster()
        {
            string infoDir = FindUnityHeroesDirectory();
            if (infoDir == null)
                Assert.Ignore("unity content directory not found");

            List<string> contents = Directory.GetFiles(infoDir, "*.bytes")
                .OrderBy(path => path)
                .Select(File.ReadAllText)
                .ToList();
            var catalog = HeroCatalog.Load(contents);

            Assert.That(catalog.ClassIds.Count, Is.GreaterThanOrEqualTo(15));
            Assert.That(catalog.ClassIds, Does.Contain("crusader"));
            Assert.That(catalog.ClassIds, Does.Contain("vestal"));
            foreach (string id in catalog.ClassIds)
            {
                HeroClass heroClass;
                Assert.That(catalog.TryGet(id, out heroClass), Is.True);
                Assert.That(heroClass.CombatSkills.Count, Is.GreaterThan(0), id + " has no combat skills");
            }
        }

        [Test]
        public void Catalog_SkipsUnparsableFiles_AndKeepsOrder()
        {
            var catalog = HeroCatalog.Load(new[] { "garbage", Sample });

            Assert.That(catalog.ClassIds.Count, Is.EqualTo(1));
            Assert.That(catalog.ClassIds[0], Is.EqualTo("test_knight"));

            HeroClass found;
            Assert.That(catalog.TryGet("test_knight", out found), Is.True);
            Assert.That(catalog.TryGet("missing", out found), Is.False);
        }

        private static string FindUnityHeroesDirectory()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                string candidate = Path.Combine(
                    current.FullName, "unity", "Assets", "Resources", "Data", "Heroes", "Info");
                if (Directory.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return null;
        }
    }
}
