namespace Sektor.DarkestDungeon.Core.Content.Tests.Database
{
    using System.IO;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Content.Database;

    [TestFixture]
    public class LootMapperTests
    {
        [Test]
        public void Parse_RealContent_YieldsDarknessBonuses()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "JsonLoot.json"));
            var jsonLoot = JsonConvert.DeserializeObject<JsonLootDatabase>(jsonText);

            var loot = LootMapper.Parse(jsonLoot);

            Assert.That(loot.DarknessLoot, Does.ContainKey("battle"));
            Assert.That(loot.DarknessLoot, Does.ContainKey("chest"));
        }

        [Test]
        public void Parse_RealContent_YieldsLootTables()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "JsonLoot.json"));
            var jsonLoot = JsonConvert.DeserializeObject<JsonLootDatabase>(jsonText);

            var loot = LootMapper.Parse(jsonLoot);

            Assert.That(loot.LootTables, Has.Count.GreaterThan(0));
            Assert.That(loot.LootTables, Does.ContainKey("A"));
            Assert.That(loot.LootTables["A"], Has.Count.GreaterThan(0));
        }
    }
}
