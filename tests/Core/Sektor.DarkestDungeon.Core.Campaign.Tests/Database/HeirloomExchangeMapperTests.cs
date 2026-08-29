using System.IO;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Campaign.Database;

namespace Sektor.DarkestDungeon.Core.Campaign.Tests.Database
{
    [TestFixture]
    public class HeirloomExchangeMapperTests
    {
        [Test]
        public void Parse_RealContent_YieldsAllExchangeRates()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "Mechanics", "HeirloomExchange.json"));
            var jsonExchange = JsonConvert.DeserializeObject<JsonHeirloomExchange>(jsonText);

            var exchanges = HeirloomExchangeMapper.Parse(jsonExchange);

            Assert.That(exchanges, Has.Count.EqualTo(12));
        }

        [Test]
        public void Parse_RealContent_BustToPortraitMatchesContent()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "Mechanics", "HeirloomExchange.json"));
            var jsonExchange = JsonConvert.DeserializeObject<JsonHeirloomExchange>(jsonText);

            var exchanges = HeirloomExchangeMapper.Parse(jsonExchange);

            Assert.That(exchanges[0].FromType, Is.EqualTo("bust"));
            Assert.That(exchanges[0].FromAmount, Is.EqualTo(3));
            Assert.That(exchanges[0].ToType, Is.EqualTo("portrait"));
            Assert.That(exchanges[0].ToAmount, Is.EqualTo(1));
        }
    }
}
