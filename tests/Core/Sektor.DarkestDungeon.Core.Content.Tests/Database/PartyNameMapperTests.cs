using System.IO;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Content.Tests.Database
{
    [TestFixture]
    public class PartyNameMapperTests
    {
        [Test]
        public void Parse_RealContent_YieldsEntries()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "PartyNames.json"));
            var jsonPartyNames = JsonConvert.DeserializeObject<JsonPartyNameDictionary>(jsonText);

            var partyNames = PartyNameMapper.Parse(jsonPartyNames);

            Assert.That(partyNames, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Parse_RealContent_FirstEntryHasIdAndClasses()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "PartyNames.json"));
            var jsonPartyNames = JsonConvert.DeserializeObject<JsonPartyNameDictionary>(jsonText);

            var partyNames = PartyNameMapper.Parse(jsonPartyNames);

            Assert.That(partyNames[0].Id, Is.EqualTo("0"));
            Assert.That(partyNames[0].ClassIds, Is.EqualTo(new[] { "vestal", "plague_doctor", "highwayman", "crusader" }));
        }
    }
}
