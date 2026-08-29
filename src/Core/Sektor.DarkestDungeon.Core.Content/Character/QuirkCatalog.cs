using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Content.Character
{
    /// <summary>Loads the hero quirk catalog from the campaign JsonQuirks.json content.</summary>
    public sealed class QuirkCatalog
    {
        private readonly Dictionary<string, Quirk> _byId = new Dictionary<string, Quirk>();

        private QuirkCatalog()
        {
            Positive = new List<Quirk>();
            Negative = new List<Quirk>();
        }

        /// <summary>Gets the empty catalog (no quirks).</summary>
        public static QuirkCatalog Empty { get { return new QuirkCatalog(); } }

        /// <summary>Gets the positive quirks.</summary>
        public List<Quirk> Positive { get; private set; }

        /// <summary>Gets the negative quirks.</summary>
        public List<Quirk> Negative { get; private set; }

        /// <summary>Gets all quirks ordered by their wire order.</summary>
        public IReadOnlyCollection<Quirk> All { get { return _byId.Values; } }

        /// <summary>Maps the deserialized JsonQuirks.json document into a quirk catalog.</summary>
        /// <param name="data">The deserialized JsonQuirks.json document.</param>
        /// <returns>The quirk catalog.</returns>
        public static QuirkCatalog Load(JsonQuirkData data)
        {
            var catalog = new QuirkCatalog { };
            if (data?.quirks == null)
                return catalog;

            foreach (Quirk quirk in QuirkMapper.Parse(data.quirks))
            {
                catalog._byId[quirk.Id] = quirk;
                if (quirk.IsPositive)
                    catalog.Positive.Add(quirk);
                else
                    catalog.Negative.Add(quirk);
            }

            return catalog;
        }

        /// <summary>Gets a quirk by id, or null when the id is unknown.</summary>
        /// <param name="id">The quirk id.</param>
        /// <returns>The quirk or null.</returns>
        public Quirk Get(string id)
        {
            Quirk quirk;
            return _byId.TryGetValue(id, out quirk) ? quirk : null;
        }
    }
}