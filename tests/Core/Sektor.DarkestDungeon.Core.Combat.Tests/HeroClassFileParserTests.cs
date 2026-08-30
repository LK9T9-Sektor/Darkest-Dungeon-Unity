using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
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

            string effectsPath = Path.Combine(infoDir, "..", "..", "Mechanics", "Effects.txt");
            var effects = EffectCatalog.Load(
                File.Exists(effectsPath) ? File.ReadAllText(Path.GetFullPath(effectsPath)) : string.Empty);
            var catalog = HeroCatalog.Load(contents, effects);

            Assert.That(catalog.ClassIds.Count, Is.GreaterThanOrEqualTo(15));
            Assert.That(catalog.ClassIds, Does.Contain("crusader"));
            Assert.That(catalog.ClassIds, Does.Contain("vestal"));

            int skillsWithEffects = 0;
            int totalSkills = 0;
            foreach (string id in catalog.ClassIds)
            {
                HeroClass heroClass;
                Assert.That(catalog.TryGet(id, out heroClass), Is.True);
                Assert.That(heroClass.CombatSkills.Count, Is.GreaterThan(0), id + " has no combat skills");
                foreach (var skill in heroClass.CombatSkills)
                {
                    totalSkills++;
                    if (skill.Effects.Count > 0)
                        skillsWithEffects++;
                }
            }

            Assert.That(skillsWithEffects, Is.GreaterThan(0),
                "real hero skills should resolve at least one effect from the effects catalog");
            Assert.That(skillsWithEffects, Is.GreaterThan(totalSkills / 2),
                "most real hero skills should carry effects (" + skillsWithEffects + "/" + totalSkills + ")");

            HeroClass abomination;
            if (catalog.TryGet("abomination", out abomination))
            {
                Assert.That(abomination.Modes.Count, Is.EqualTo(2), "The abomination declares human and beast modes.");
                var transform = abomination.CombatSkills.First(skill => skill.Id == "transform");
                Assert.That(transform.ModeEffects.Count, Is.EqualTo(2));
                Assert.That(transform.ModeEffects["human"].Count, Is.EqualTo(4));
                Assert.That(transform.ModeEffects["beast"].Count, Is.EqualTo(4));
                Assert.That(transform.ModeEffects["beast"].Count, Is.EqualTo(4));
                Assert.That(transform.Category, Is.EqualTo(SkillCategory.Support));
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

        [Test]
        public void Parse_ResolvesSkillEffects_FromEffectsCatalog()
        {
            var effects = EffectCatalog.Load(
                "effect: .name \"Stun 1\" .target \"target\" .chance 100% .stun 1\n" +
                "effect: .name \"Bleed 1\" .target \"target\" .chance 110% .dotBleed 1 .duration 3");

            var heroClass = HeroClassFileParser.Parse(Sample, effects);

            Assert.That(heroClass, Is.Not.Null);
            var smite = heroClass.CombatSkills.Single(skill => skill.Id == "smite");
            Assert.That(smite.Effects.Count, Is.EqualTo(0));

            var stunned = HeroClassFileParser.Parse(
                Sample + "combat_skill: .id \"bash\" .level 0 .type \"melee\" .atk 85% .dmg -40% .launch 21 .target 12 .effect \"Stun 1\" \"Bleed 1\"",
                effects);
            var bash = stunned.CombatSkills.Single(skill => skill.Id == "bash");
            Assert.That(bash.Effects.Count, Is.EqualTo(2));
            Assert.That(bash.Effects[0].SubEffects.Any(sub => sub.Type == EffectSubType.Stun), Is.True);
            Assert.That(bash.Effects[1].SubEffects.Any(sub => sub.Type == EffectSubType.Bleeding), Is.True);
        }

        [Test]
        public void Parse_ReadsSkillLimitsAndContinueTurn()
        {
            var effects = EffectCatalog.Load(string.Empty);
            var heroClass = HeroClassFileParser.Parse(Sample, effects);

            var limited = HeroClassFileParser.Parse(
                Sample + "combat_skill: .id \"bellow\" .level 0 .type \"melee\" .atk 85% .dmg -40% .launch 321 .target .is_crit_valid False .is_continue_turn true .per_turn_limit 1 .per_battle_limit 2",
                effects);

            var bellow = limited.CombatSkills.Single(skill => skill.Id == "bellow");
            Assert.That(bellow.LimitPerTurn, Is.EqualTo(1));
            Assert.That(bellow.LimitPerBattle, Is.EqualTo(2));
            Assert.That(bellow.IsContinueTurn, Is.True);
        }

        [Test]
        public void Parse_ReadsModesValidModesAndModeEffects()
        {
            var effects = EffectCatalog.Load(
                "effect: .name \"Switch Beast\" .target \"performer\" .set_mode beast\n" +
                "effect: .name \"Beast Buff\" .target \"performer\" .combat_stat_buff 1 .attack_rating_add 10%\n" +
                "effect: .name \"Stress Party\" .target \"performer_group_other\" .stress 8");

            const string Content = @"
name: abom_test
art:
combat_skill: .id ""transform"" .icon ""one""
.end
info:
resistances: .stun 40%
weapon: .name ""w"" .atk 0% .dmg 6 11 .crit 2.5% .spd 7
armour: .name ""a"" .def 5% .prot 0 .hp 33 .spd 0
mode: .id human .is_raid_default true
mode: .id beast
combat_skill: .id ""transform"" .level 0 .type ""ranged"" .atk 0% .dmg 0% .crit 0% .launch 321 .target  .is_crit_valid True .valid_modes human beast .human_effects ""Switch Beast"" ""Beast Buff"" .beast_effects ""Stress Party"" .is_continue_turn true .per_battle_limit 2
combat_skill: .id ""rage"" .level 0 .type ""melee"" .atk 85% .dmg 0% .crit 5% .launch 21 .target 123 .is_crit_valid True .valid_modes beast
.end";

            var heroClass = HeroClassFileParser.Parse(Content, effects);

            Assert.That(heroClass, Is.Not.Null);
            Assert.That(heroClass.Modes.Count, Is.EqualTo(2));
            Assert.That(heroClass.Modes[0].Id, Is.EqualTo("human"));
            Assert.That(heroClass.Modes[0].IsRaidDefault, Is.True);
            Assert.That(heroClass.Modes[1].Id, Is.EqualTo("beast"));
            Assert.That(heroClass.Modes[1].IsRaidDefault, Is.False);

            var transform = heroClass.CombatSkills.Single(skill => skill.Id == "transform");
            Assert.That(transform.Category, Is.EqualTo(SkillCategory.Support),
                "Accuracy-0 self-target skills should be support (no accuracy roll).");
            CollectionAssert.AreEquivalent(new[] { "human", "beast" }, transform.ValidModes);
            Assert.That(transform.IsContinueTurn, Is.True);
            Assert.That(transform.LimitPerBattle, Is.EqualTo(2));
            Assert.That(transform.ModeEffects["human"].Count, Is.EqualTo(2));
            Assert.That(transform.ModeEffects["beast"].Count, Is.EqualTo(1));

            var rage = heroClass.CombatSkills.Single(skill => skill.Id == "rage");
            CollectionAssert.AreEquivalent(new[] { "beast" }, rage.ValidModes);
            Assert.That(rage.Category, Is.EqualTo(SkillCategory.Damage));
        }

        [Test]
        public void Parse_ReadsDeathsDoorBuffsAndRecoveryBuffs()
        {
            var content = Sample + @"
deaths_door: .buffs deathsdoorACCDebuff deathsdoorDMGLowDebuff .recovery_buffs mortalityACCDebuff .recovery_heart_attack_buffs heartattackACCDebuff
";
            var heroClass = HeroClassFileParser.Parse(content);

            Assert.That(heroClass, Is.Not.Null);
            Assert.That(heroClass.DeathDoor, Is.Not.Null);
            CollectionAssert.AreEquivalent(
                new[] { "deathsdooraccdebuff", "deathsdoordmglowdebuff" }, heroClass.DeathDoor.Buffs);
            CollectionAssert.AreEquivalent(new[] { "mortalityaccdebuff" }, heroClass.DeathDoor.RecoveryBuffs);
            CollectionAssert.AreEquivalent(new[] { "heartattackaccdebuff" }, heroClass.DeathDoor.HeartAttackBuffs);
        }

        [Test]
        public void Parse_RealRoster_HasDeathsDoorData()
        {
            string directory = FindUnityHeroesDirectory();
            if (directory == null)
                Assert.Ignore("Unity heroes content not available in this environment.");

            var content = File.ReadAllText(Path.Combine(directory, "Crusader.bytes"));
            var heroClass = HeroClassFileParser.Parse(content);

            Assert.That(heroClass, Is.Not.Null);
            Assert.That(heroClass.DeathDoor, Is.Not.Null, "The crusader should carry death's door data.");
            Assert.That(heroClass.DeathDoor.Buffs, Has.Count.GreaterThan(0), "Death's door debuffs should be parsed.");
            Assert.That(heroClass.DeathDoor.RecoveryBuffs, Has.Count.GreaterThan(0), "Mortality buffs should be parsed.");
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
