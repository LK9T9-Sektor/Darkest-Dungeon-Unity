namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Content.Database;

    [TestFixture]
    public class QuirkBuffTests
    {
        [Test]
        public void EveryQuirk_AppliesInDuelWithoutNullAttribute()
        {
            var content = new TestDuelContent();

            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Quirks", "JsonQuirks.json");
            var data = JsonConvert.DeserializeObject<JsonQuirkData>(File.ReadAllText(path));

            foreach (var quirk in QuirkMapper.Parse(data.quirks))
            {
                var duel = new DuelController(content);
                var picks = new[]
                {
                    new DuelHeroPick("crusader", 1, null, new[] { quirk.Id }),
                    new DuelHeroPick("crusader", 2),
                    new DuelHeroPick("crusader", 3),
                    new DuelHeroPick("crusader", 4),
                };

                Assert.DoesNotThrow(() => duel.StartDuel(picks, Picks("highwayman"), 42, isHost: true),
                    "Quirk " + quirk.Id + " should apply its buffs without a null attribute.");
            }
        }

        private static DuelHeroPick[] Picks(string classId)
        {
            return new[]
            {
                new DuelHeroPick(classId, 1),
                new DuelHeroPick(classId, 2),
                new DuelHeroPick(classId, 3),
                new DuelHeroPick(classId, 4),
            };
        }
    }
}