using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Content.Tests.Database
{
    [TestFixture]
    public class BuffContentMapperTests
    {
        private static List<JsonBuff> LoadJsonBuffs()
        {
            string jsonText = File.ReadAllText(Path.Combine("Data", "JsonBuffs.json"));
            return JsonConvert.DeserializeObject<JsonBuffData>(jsonText).buffs;
        }

        [Test]
        public void Parse_RealContent_YieldsBuffs()
        {
            var buffs = BuffContentMapper.Parse(LoadJsonBuffs());

            Assert.That(buffs, Has.Count.GreaterThan(0));
            Assert.That(buffs.Select(b => b.Id), Does.Contain("MAXHP10"));
            Assert.That(buffs.Select(b => b.Id), Does.Contain("PROT10"));
        }

        [Test]
        public void Parse_RealContent_MAXHP10IsMultiplyOnMaxHp()
        {
            var buffs = BuffContentMapper.Parse(LoadJsonBuffs());
            var maxHp = buffs.First(b => b.Id == "MAXHP10");

            Assert.That(maxHp.StatType, Is.EqualTo("combat_stat_multiply"));
            Assert.That(maxHp.AttributeTypeName, Is.EqualTo("max_hp"));
            Assert.That(maxHp.Amount, Is.EqualTo(0.1f));
            Assert.That(maxHp.RuleTypeName, Is.EqualTo("always"));
        }

        [Test]
        public void Parse_RealContent_PreservesNegativeQuirkBuffs()
        {
            var buffs = BuffContentMapper.Parse(LoadJsonBuffs());
            var minusHp = buffs.First(b => b.Id == "MAXHP-10");

            Assert.That(minusHp.Amount, Is.EqualTo(-0.1f));
        }
    }
}