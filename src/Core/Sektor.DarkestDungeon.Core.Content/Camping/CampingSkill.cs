using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Camping
{
    /// <summary>A camping skill a hero pack can use during the camp rest phase.</summary>
    public sealed class CampingSkill
    {
        /// <summary>Initializes a new instance of the <see cref="CampingSkill"/> class.</summary>
        /// <param name="id">The skill id.</param>
        /// <param name="level">The skill level.</param>
        /// <param name="cost">The camping point cost.</param>
        /// <param name="useLimit">The use limit per camping session.</param>
        /// <param name="heroClasses">The hero classes that can learn the skill.</param>
        /// <param name="buffIds">The buff ids applied while camping.</param>
        public CampingSkill(string id, int level, int cost, int useLimit, IReadOnlyList<string> heroClasses, IReadOnlyList<string> buffIds)
        {
            Id = id;
            Level = level;
            Cost = cost;
            UseLimit = useLimit;
            HeroClasses = heroClasses;
            BuffIds = buffIds;
        }

        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the skill level.</summary>
        public int Level { get; }

        /// <summary>Gets the camping point cost.</summary>
        public int Cost { get; }

        /// <summary>Gets the use limit per camping session.</summary>
        public int UseLimit { get; }

        /// <summary>Gets the hero classes that can learn the skill.</summary>
        public IReadOnlyList<string> HeroClasses { get; }

        /// <summary>Gets the buff ids applied while camping.</summary>
        public IReadOnlyList<string> BuffIds { get; }
    }
}