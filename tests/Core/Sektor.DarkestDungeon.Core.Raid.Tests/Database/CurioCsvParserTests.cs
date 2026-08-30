using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Raid.Database;

namespace Sektor.DarkestDungeon.Core.Raid.Tests.Database
{
    [TestFixture]
    public class CurioCsvParserTests
    {
        [Test]
        public void Parse_RealContent_YieldsCurios()
        {
            string csvText = File.ReadAllText(Path.Combine("Data", "Curios", "Curios.csv"));

            var curios = CurioCsvParser.Parse(csvText);

            Assert.That(curios, Has.Count.GreaterThan(0));
            Assert.That(curios, Does.ContainKey("unlocked_strongbox"));
        }

        [Test]
        public void Parse_RealContent_UnlockedStrongboxHasFields()
        {
            string csvText = File.ReadAllText(Path.Combine("Data", "Curios", "Curios.csv"));

            var curios = CurioCsvParser.Parse(csvText);
            var strongbox = curios["unlocked_strongbox"];

            Assert.That(strongbox.IsFullCurio, Is.True);
            Assert.That(strongbox.Tags, Does.Contain("treasure"));
            Assert.That(strongbox.Results, Has.Count.GreaterThan(0));
        }
    }
}
