using System.Collections.Generic;
using Newtonsoft.Json;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Campaign;
using Sektor.DarkestDungeon.Core.Content.Database;
using Sektor.DarkestDungeon.Core.Content.Raid;
using Sektor.DarkestDungeon.Core.Data.Catalogs;
using Sektor.DarkestDungeon.Core.Data.Dto;

namespace Sektor.DarkestDungeon.Core.Data.Readers
{
    /// <summary>
    /// Single reader facade over the whole campaign data set: raw JSON bundles, mapped content
    /// models, and combat-ready catalogs. Every method takes content text so any source can feed
    /// it (files, Unity TextAssets, embedded resources).
    /// </summary>
    public static class GameDataReader
    {
        /// <summary>Deserializes the Data\JsonCamping.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The camping document.</returns>
        public static JsonCamping ReadCamping(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonCamping>(jsonText);
        }

        /// <summary>Deserializes the Data\JsonQuests.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The quests document.</returns>
        public static JsonQuests ReadQuests(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonQuests>(jsonText);
        }

        /// <summary>Deserializes the Data\JsonTrinkets.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The trinkets document.</returns>
        public static JsonTrinkets ReadTrinkets(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonTrinkets>(jsonText);
        }

        /// <summary>Deserializes the Data\Mechanics\Provision.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The provision document.</returns>
        public static JsonProvision ReadProvision(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonProvision>(jsonText);
        }

        /// <summary>Deserializes the Data\Mechanics\Roster.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The roster document.</returns>
        public static JsonRoster ReadRoster(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonRoster>(jsonText);
        }

        /// <summary>Deserializes the Data\Mechanics\TownEvents.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The town events document.</returns>
        public static JsonTownEvents ReadTownEvents(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonTownEvents>(jsonText);
        }

        /// <summary>Deserializes the Data\Mechanics\Campaign.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The campaign document.</returns>
        public static JsonCampaign ReadCampaign(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonCampaign>(jsonText);
        }

        /// <summary>Deserializes a Data\Upgrades\*\*.upgrades.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The upgrades document.</returns>
        public static JsonUpgrades ReadUpgrades(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonUpgrades>(jsonText);
        }

        /// <summary>Deserializes a Data\Buildings\*.building.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The building document.</returns>
        public static JsonBuilding ReadBuilding(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonBuilding>(jsonText);
        }

        /// <summary>Deserializes a Data\Curios\*.json prop bundle (Obstacles or Traps).</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The props document.</returns>
        public static JsonCurioProps ReadCurioProps(string jsonText)
        {
            return JsonConvert.DeserializeObject<JsonCurioProps>(jsonText);
        }

        /// <summary>Maps the Data\JsonTraits.json bundle into trait definitions.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The trait list.</returns>
        public static List<Trait> ReadTraits(string jsonText)
        {
            JsonTraitData data = JsonConvert.DeserializeObject<JsonTraitData>(jsonText);
            return data == null ? new List<Trait>() : TraitMapper.Parse(data.traits);
        }

        /// <summary>Parses the curio props CSV database.</summary>
        /// <param name="csvText">The CSV file content.</param>
        /// <returns>The curio props keyed by id.</returns>
        public static Dictionary<string, Curio> ReadCurios(string csvText)
        {
            return CurioCsvParser.Parse(csvText);
        }

        /// <summary>Maps the Data\JsonLoot.json bundle into the loot database.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The loot database.</returns>
        public static LootDatabase ReadLoot(string jsonText)
        {
            JsonLootDatabase data = JsonConvert.DeserializeObject<JsonLootDatabase>(jsonText);
            return data == null ? new LootDatabase() : LootMapper.Parse(data);
        }

        /// <summary>Maps the Data\Narration.json bundle into narration entries.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The narration entries keyed by id.</returns>
        public static Dictionary<string, NarrationEntry> ReadNarration(string jsonText)
        {
            JsonNarration data = JsonConvert.DeserializeObject<JsonNarration>(jsonText);
            return data == null ? new Dictionary<string, NarrationEntry>() : NarrationMapper.Parse(data);
        }

        /// <summary>Maps the Data\PartyNames.json bundle into party name entries.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The party name entries.</returns>
        public static List<PartyNameEntry> ReadPartyNames(string jsonText)
        {
            JsonPartyNameDictionary data = JsonConvert.DeserializeObject<JsonPartyNameDictionary>(jsonText);
            return data == null ? new List<PartyNameEntry>() : PartyNameMapper.Parse(data);
        }

        /// <summary>Maps the Data\Mechanics\HeirloomExchange.json bundle into exchange entries.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The heirloom exchange entries.</returns>
        public static List<HeirloomExchange> ReadHeirloomExchange(string jsonText)
        {
            JsonHeirloomExchange data = JsonConvert.DeserializeObject<JsonHeirloomExchange>(jsonText);
            return data == null ? new List<HeirloomExchange>() : HeirloomExchangeMapper.Parse(data);
        }

        /// <summary>Builds the monster brain catalog from the Data\JsonAI.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The monster brain catalog.</returns>
        public static MonsterBrainCatalog ReadBrains(string jsonText)
        {
            return MonsterBrainCatalog.Load(jsonText);
        }

        /// <summary>Builds the buff catalog from the Data\JsonBuffs.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The buff catalog.</returns>
        public static BuffCatalog ReadBuffs(string jsonText)
        {
            return BuffCatalog.Load(jsonText);
        }

        /// <summary>Builds the quirk catalog from the Data\JsonQuirks.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The quirk catalog.</returns>
        public static QuirkCatalog ReadQuirks(string jsonText)
        {
            return QuirkCatalog.Load(jsonText);
        }

        /// <summary>Builds the trinket catalog from the Data\JsonTrinkets.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The trinket catalog.</returns>
        public static TrinketCatalog ReadTrinketCatalog(string jsonText)
        {
            return TrinketCatalog.Load(ReadTrinkets(jsonText));
        }

        /// <summary>Builds the camping skill catalog from the Data\JsonCamping.json bundle.</summary>
        /// <param name="jsonText">The file content.</param>
        /// <returns>The camping skill catalog.</returns>
        public static CampingSkillCatalog ReadCampingSkills(string jsonText)
        {
            return CampingSkillCatalog.Load(ReadCamping(jsonText));
        }

        /// <summary>Builds the effect catalog from the raw effects database text.</summary>
        /// <param name="content">The Effects database text.</param>
        /// <returns>The effect catalog.</returns>
        public static EffectCatalog ReadEffects(string content)
        {
            return EffectCatalog.Load(content);
        }

        /// <summary>Builds the hero class catalog from the hero class file contents.</summary>
        /// <param name="heroClassFiles">The hero class file contents.</param>
        /// <param name="effects">The shared effect catalog.</param>
        /// <returns>The hero class catalog.</returns>
        public static HeroCatalog ReadHeroes(IEnumerable<string> heroClassFiles, EffectCatalog effects)
        {
            return HeroCatalog.Load(heroClassFiles, effects);
        }

        /// <summary>Builds the monster class catalog from the monster file contents.</summary>
        /// <param name="monsterFiles">The monster file contents.</param>
        /// <param name="effects">The shared effect catalog.</param>
        /// <returns>The monster class catalog.</returns>
        public static MonsterCatalog ReadMonsters(IEnumerable<string> monsterFiles, EffectCatalog effects)
        {
            return MonsterCatalog.Load(monsterFiles, effects);
        }
    }
}