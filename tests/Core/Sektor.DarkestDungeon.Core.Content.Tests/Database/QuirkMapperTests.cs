using System.IO;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Content.Tests.Database
{
    [TestFixture]
    public class QuirkMapperTests
    {
        [Test]
        public void Parse_RealContent_YieldsQuirks()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "JsonQuirks.json"));
            var jsonQuirks = JsonConvert.DeserializeObject<JsonQuirkData>(jsonText);

            var quirks = QuirkMapper.Parse(jsonQuirks.quirks);

            Assert.That(quirks, Has.Count.GreaterThan(0));
            Assert.That(quirks.Select(q => q.Id), Does.Contain("tough"));
            Assert.That(quirks.Select(q => q.Id), Does.Contain("fragile"));
        }

        [Test]
        public void Parse_RealContent_PositiveAndNegativeFlags()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "JsonQuirks.json"));
            var jsonQuirks = JsonConvert.DeserializeObject<JsonQuirkData>(jsonText);

            var quirks = QuirkMapper.Parse(jsonQuirks.quirks);

            var tough = quirks.First(q => q.Id == "tough");
            var fragile = quirks.First(q => q.Id == "fragile");

            Assert.That(tough.IsPositive, Is.True);
            Assert.That(tough.Buffs, Does.Contain("MAXHP10"));
            Assert.That(fragile.IsPositive, Is.False);
            Assert.That(fragile.Buffs, Does.Contain("MAXHP-10"));
            Assert.That(tough.IncompatibleQuirks, Does.Contain("fragile"));
        }
    }
}