using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Data.Readers;

namespace Sektor.DarkestDungeon.Core.Data.Catalogs
{
    /// <summary>Catalog of monster brains keyed by their identifier.</summary>
    public sealed class MonsterBrainCatalog
    {
        private readonly Dictionary<string, MonsterBrain> _brains;

        /// <summary>Initializes a new instance of the <see cref="MonsterBrainCatalog"/> class.</summary>
        /// <param name="brains">The monster brains to index.</param>
        public MonsterBrainCatalog(IEnumerable<MonsterBrain> brains)
        {
            _brains = new Dictionary<string, MonsterBrain>();

            if (brains == null)
                return;

            foreach (MonsterBrain brain in brains)
                _brains[brain.Id] = brain;
        }

        /// <summary>Gets the number of brains in the catalog.</summary>
        public int Count
        {
            get { return _brains.Count; }
        }

        /// <summary>Loads the catalog from the JsonAI.json file content.</summary>
        /// <param name="jsonText">The JsonAI.json file content.</param>
        /// <returns>A catalog containing all parsed brains.</returns>
        public static MonsterBrainCatalog Load(string jsonText)
        {
            return new MonsterBrainCatalog(new JsonBrainParser().Parse(jsonText));
        }

        /// <summary>Gets the brain with the given identifier.</summary>
        /// <param name="id">The brain identifier.</param>
        /// <param name="brain">The matching brain when found.</param>
        /// <returns>True when a brain with the given identifier exists.</returns>
        public bool TryGet(string id, out MonsterBrain brain)
        {
            return _brains.TryGetValue(id, out brain);
        }
    }
}