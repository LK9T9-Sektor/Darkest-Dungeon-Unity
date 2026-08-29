using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;

namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics.AI
{
    /// <summary>Tests for the JsonAI.json brain parser and catalog.</summary>
    public class JsonBrainParserTests
    {
        /// <summary>Parsing an empty document yields no brains and does not throw.</summary>
        [Test]
        public void ParseEmptyReturnsNoBrains()
        {
            var parser = new JsonBrainParser();

            var brains = parser.Parse(JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>("{}"));

            Assert.That(brains, Is.Empty);
        }

        /// <summary>Cooldowns and desire wire-keys map to the correct core desire types.</summary>
        [Test]
        public void ParseMapsCooldownsAndTypedDesires()
        {
            const string Json =
                @"{
                    ""monster_brains"": [
                        {
                            ""id"": ""b1"",
                            ""skill_cooldowns"": [ { ""combat_skill_id"": ""clapperclaw"", ""amount"": 2 } ],
                            ""skill_selection_desires"": [
                                { ""type"": ""random_skill"", ""data"": { ""base_chance"": 1.0 } },
                                { ""type"": ""specific_skill"", ""data"": { ""combat_skill_id"": ""claw"", ""per_round_chance"": 0.1 } }
                            ],
                            ""target_selection_desires"": [
                                { ""type"": ""rank_target"", ""data"": { ""rank"": 3, ""is_enemy_target_desire"": true } }
                            ],
                            ""bonus_initiative_desires"": [
                                { ""type"": ""guaranteed"", ""data"": { ""monsters_min"": 1 } }
                            ]
                        }
                    ]
                }";

            var parser = new JsonBrainParser();
            var brains = parser.Parse(JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>(Json));

            Assert.That(brains, Has.Count.EqualTo(1));
            MonsterBrain brain = brains[0];
            Assert.That(brain.Id, Is.EqualTo("b1"));
            Assert.That(brain.SkillCooldowns, Has.Count.EqualTo(1));
            Assert.That(brain.SkillCooldowns[0].SkillId, Is.EqualTo("clapperclaw"));
            Assert.That(brain.SkillCooldowns[0].Amount, Is.EqualTo(2));
            Assert.That(brain.SkillDesireSet[0], Is.TypeOf<SkillSelectionRandom>());
            Assert.That(brain.SkillDesireSet[1], Is.TypeOf<SkillSelectionSpecific>());
            Assert.That(brain.TargetDesireSet[0], Is.TypeOf<TargetSelectionRank>());
            Assert.That(brain.BonusDesireSet[0], Is.TypeOf<BonusInitiativeGuaranteed>());
        }

        /// <summary>Unknown desire wire-keys are skipped without throwing.</summary>
        [Test]
        public void ParseSkipsUnknownDesireTypes()
        {
            const string Json =
                @"{
                    ""monster_brains"": [
                        {
                            ""id"": ""b2"",
                            ""skill_selection_desires"": [
                                { ""type"": ""not_a_desire"", ""data"": { } },
                                { ""type"": ""random_skill"", ""data"": { ""base_chance"": 1.0 } }
                            ]
                        }
                    ]
                }";

            var parser = new JsonBrainParser();
            var brains = parser.Parse(JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>(Json));

            Assert.That(brains, Has.Count.EqualTo(1));
            Assert.That(brains[0].SkillDesireSet, Has.Count.EqualTo(1));
            Assert.That(brains[0].SkillDesireSet[0], Is.TypeOf<SkillSelectionRandom>());
        }

        /// <summary>The real JsonAI.json campaign file parses into a populated catalog.</summary>
        [Test]
        public void CatalogLoadsRealCampaignJson()
        {
            string aiPath = FindUnityDataFile("Data", "JsonAI.json");
            Assert.That(aiPath, Is.Not.Null, "unity Assets/Resources/Data/JsonAI.json must be available.");

            JsonMonsterBrainsDatabase root = JsonConvert.DeserializeObject<JsonMonsterBrainsDatabase>(File.ReadAllText(aiPath));
            var catalog = new MonsterBrainCatalog(new JsonBrainParser().Parse(root));

            Assert.That(catalog.Count, Is.GreaterThan(0));
            Assert.That(catalog.TryGet("default", out MonsterBrain brain), Is.True);
            Assert.That(brain.SkillDesireSet, Is.Not.Empty);
            Assert.That(brain.TargetDesireSet, Is.Not.Empty);
            Assert.That(brain.BonusDesireSet, Is.Not.Null);
        }

        private static string FindUnityDataFile(params string[] parts)
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                string candidate = Path.Combine(
                    new[] { current.FullName, "unity", "Assets", "Resources" }.Concat(parts).ToArray());
                if (File.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return null;
        }
    }
}