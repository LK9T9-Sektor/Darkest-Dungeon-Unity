using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Catalog of monster classes indexed by string identifier.</summary>
    public sealed class MonsterCatalog
    {
        private readonly Dictionary<string, MonsterClass> _monsters;

        /// <summary>Initializes a new instance of the <see cref="MonsterCatalog"/> class.</summary>
        /// <param name="monsters">The monster classes to cache.</param>
        public MonsterCatalog(IEnumerable<MonsterClass> monsters)
        {
            _monsters = new Dictionary<string, MonsterClass>();
            if (monsters == null)
                return;

            foreach (var monster in monsters)
                if (monster != null && !string.IsNullOrEmpty(monster.StringId))
                    _monsters[monster.StringId] = monster;
        }

        /// <summary>Gets the cached monster count.</summary>
        public int Count { get { return _monsters.Count; } }

        /// <summary>Gets the monster string ids in the catalog.</summary>
        public IReadOnlyList<string> Ids { get { return _monsters.Keys.OrderBy(id => id).ToList(); } }

        /// <summary>Loads monster classes from file contents.</summary>
        /// <param name="fileContents">The monster .txt file contents.</param>
        /// <param name="effects">The effect catalog used to resolve skill effects (optional).</param>
        /// <returns>The populated catalog.</returns>
        public static MonsterCatalog Load(IEnumerable<string> fileContents, EffectCatalog effects = null)
        {
            var classes = new List<MonsterClass>();
            if (fileContents != null)
            {
                foreach (var content in fileContents)
                {
                    if (content == null)
                        continue;

                    var monsterClass = MonsterClassFileParser.Parse(content, effects);
                    if (monsterClass != null)
                        classes.Add(monsterClass);
                }
            }
            return new MonsterCatalog(classes);
        }

        /// <summary>Checks whether a monster id exists.</summary>
        /// <param name="monsterId">The monster string id.</param>
        /// <returns>True when the monster is cached.</returns>
        public bool Contains(string monsterId)
        {
            return monsterId != null && _monsters.ContainsKey(monsterId);
        }

        /// <summary>Tries to get a monster class by id.</summary>
        /// <param name="monsterId">The monster string id.</param>
        /// <param name="monster">The found monster class, or null.</param>
        /// <returns>True when the monster was found.</returns>
        public bool TryGet(string monsterId, out MonsterClass monster)
        {
            return _monsters.TryGetValue(monsterId ?? string.Empty, out monster);
        }
    }
}