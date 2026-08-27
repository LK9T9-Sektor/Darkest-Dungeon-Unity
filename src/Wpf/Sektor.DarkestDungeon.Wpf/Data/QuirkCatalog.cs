using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the hero quirk catalog from the bundled content file.</summary>
    public static class QuirkCatalog
    {
        /// <summary>Gets the positive quirks.</summary>
        public static List<Quirk> Positive { get; } = new List<Quirk>();

        /// <summary>Gets the negative quirks.</summary>
        public static List<Quirk> Negative { get; } = new List<Quirk>();

        static QuirkCatalog()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Quirks", "JsonQuirks.json");
            if (!File.Exists(path))
                return;

            var data = JsonConvert.DeserializeObject<JsonQuirkData>(File.ReadAllText(path));
            if (data?.quirks == null)
                return;

            foreach (var quirk in QuirkMapper.Parse(data.quirks))
            {
                if (quirk.IsPositive)
                    Positive.Add(quirk);
                else
                    Negative.Add(quirk);
            }
        }
    }
}