using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
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