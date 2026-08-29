using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Campaign.Database;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Content.Camping;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Trinket;

namespace Sektor.DarkestDungeon.Clients.Content.Tests
{
    /// <summary>Tests for the full GameDataReader against the real campaign data files.</summary>
    public class GameDataReaderTests
    {
        /// <summary>Every bundle under Data, Data\Mechanics, Data\Buildings, Data\Upgrades and Data\Curios deserializes.</summary>
        [Test]
        public void EveryDataBundleDeserializes()
        {
            string dataRoot = FindUnityDataDir();
            Assert.That(dataRoot, Is.Not.Null, "unity Assets/Resources/Data must be available.");

            Assert.That(GameDataReader.ReadCamping(Read("data", "JsonCamping.json", dataRoot)).skills, Is.Not.Empty);
            Assert.That(GameDataReader.ReadQuests(Read("data", "JsonQuests.json", dataRoot)).goals, Is.Not.Empty);
            Assert.That(GameDataReader.ReadTrinkets(Read("data", "JsonTrinkets.json", dataRoot)).trinkets, Is.Not.Empty);
            Assert.That(GameDataReader.ReadProvision(Read("mechanics", "Provision.json", dataRoot)).raid_starting_length_inventory_item_lists, Is.Not.Empty);
            Assert.That(GameDataReader.ReadRoster(Read("mechanics", "Roster.json", dataRoot)).resolve_level_thresholds, Is.Not.Empty);
            Assert.That(GameDataReader.ReadTownEvents(Read("mechanics", "TownEvents.json", dataRoot)).events, Is.Not.Empty);
            Assert.That(GameDataReader.ReadCampaign(Read("mechanics", "Campaign.json", dataRoot)).resolve_level_thresholds, Is.Not.Empty);
            Assert.That(GameDataReader.ReadCurioProps(Read("curios", "Traps.json", dataRoot)).props, Is.Not.Empty);
            Assert.That(GameDataReader.ReadCurioProps(Read("curios", "Obstacles.json", dataRoot)).props, Is.Not.Empty);

            string[] buildingFiles = Directory.GetFiles(Merge(dataRoot, "Data", "Buildings"), "*.building.json");
            Assert.That(GameDataReader.ReadBuilding(File.ReadAllText(buildingFiles[0])).on_start_town_visit_priority, Is.Not.EqualTo(0));

            string[] upgradesFiles = Directory.GetFiles(Merge(dataRoot, "Data", "Upgrades", "Heroes"), "*.upgrades.json");
            JsonUpgrades upgrades = GameDataReader.ReadUpgrades(File.ReadAllText(upgradesFiles[0]));
            Assert.That(upgrades.trees, Is.Not.Empty);
            Assert.That(upgrades.trees[0].requirements, Is.Not.Empty);
        }

        /// <summary>The buff and quirk catalogs map the real campaign JSON into Combat domain objects.</summary>
        [Test]
        public void ReadBuffsAndQuirksCatalogs()
        {
            string dataRoot = FindUnityDataDir();
            Assert.That(dataRoot, Is.Not.Null);

            BuffCatalog buffs = GameDataReader.ReadBuffs(Read("data", "JsonBuffs.json", dataRoot));
            Assert.That(buffs.All, Is.Not.Empty);
            Assert.That(buffs.Get("TRINKET_ACC_B1"), Is.Not.Null);

            QuirkCatalog quirks = GameDataReader.ReadQuirks(Read("data", "JsonQuirks.json", dataRoot));
            Assert.That(quirks.All, Is.Not.Empty);
            Assert.That(quirks.Positive, Is.Not.Empty);
            Assert.That(quirks.Negative, Is.Not.Empty);
        }

        /// <summary>The trinket and camping catalogs map the real campaign JSON into domain objects.</summary>
        [Test]
        public void ReadTrinketsAndCampingCatalogs()
        {
            string dataRoot = FindUnityDataDir();
            Assert.That(dataRoot, Is.Not.Null);

            TrinketCatalog trinkets = GameDataReader.ReadTrinketCatalog(Read("data", "JsonTrinkets.json", dataRoot));
            Assert.That(trinkets.All, Is.Not.Empty);
            Assert.That(trinkets.Get("ancestors_coat").Price, Is.GreaterThan(0));

            CampingSkillCatalog camping = GameDataReader.ReadCampingSkills(Read("data", "JsonCamping.json", dataRoot));
            Assert.That(camping.All, Is.Not.Empty);
            Assert.That(camping.Get("encourage"), Is.Not.Null);
            Assert.That(camping.Get("encourage").HeroClasses, Is.Not.Empty);
        }

        /// <summary>The traits, loot, narration, party names and heirloom bundles map via the existing content mappers.</summary>
        [Test]
        public void ReadMappedContentBundles()
        {
            string dataRoot = FindUnityDataDir();
            Assert.That(dataRoot, Is.Not.Null);

            Assert.That(GameDataReader.ReadTraits(Read("data", "JsonTraits.json", dataRoot)), Is.Not.Empty);
            Assert.That(GameDataReader.ReadLoot(Read("data", "JsonLoot.json", dataRoot)).LootTables, Is.Not.Empty);
            Assert.That(GameDataReader.ReadNarration(Read("data", "Narration.json", dataRoot)), Is.Not.Empty);
            Assert.That(GameDataReader.ReadPartyNames(Read("data", "PartyNames.json", dataRoot)), Is.Not.Empty);
            Assert.That(GameDataReader.ReadHeirloomExchange(Read("mechanics", "HeirloomExchange.json", dataRoot)), Is.Not.Empty);
        }

        private static string Read(string key, string fileName, string resourcesDir)
        {
            switch (key)
            {
                case "data":
                    return File.ReadAllText(Path.Combine(resourcesDir, "Data", fileName));
                case "mechanics":
                    return File.ReadAllText(Path.Combine(resourcesDir, "Data", "Mechanics", fileName));
                case "curios":
                    return File.ReadAllText(Path.Combine(resourcesDir, "Data", "Curios", fileName));
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }

        private static string Merge(string resourcesDir, params string[] parts)
        {
            var full = new[] { resourcesDir }.Concat(parts).ToArray();
            return Path.Combine(full);
        }

        private static string FindUnityDataDir()
        {
            var current = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory;
            while (current != null)
            {
                string candidate = Path.Combine(current.FullName, "unity", "Assets", "Resources");
                if (Directory.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return null;
        }
    }
}