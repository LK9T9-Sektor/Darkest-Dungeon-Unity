using System.IO;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Campaign.Database;

namespace Sektor.DarkestDungeon.Core.Campaign.Tests.Database
{
    [TestFixture]
    public class NarrationMapperTests
    {
        [Test]
        public void Parse_RealContent_YieldsEntries()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "Narration.json"));
            var jsonNarration = JsonConvert.DeserializeObject<JsonNarration>(jsonText);

            var narration = NarrationMapper.Parse(jsonNarration);

            Assert.That(narration, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Parse_RealContent_EntryHasAudioEvents()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "Narration.json"));
            var jsonNarration = JsonConvert.DeserializeObject<JsonNarration>(jsonText);

            var narration = NarrationMapper.Parse(jsonNarration);

            Assert.That(narration, Has.Count.GreaterThan(0));
            var first = narration.Values.GetEnumerator();
            first.MoveNext();
            Assert.That(first.Current.AudioEvents, Is.Not.Null);
        }
    }
}
