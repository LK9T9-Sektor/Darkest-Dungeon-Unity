using Sektor.DarkestDungeon.Core.Combat.Character.Components;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A hero combat mode (e.g. religious/alternation).</summary>
    public class CharacterMode : ICharacterMode
    {
        /// <summary>Gets or sets the mode identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets a value indicating whether this is the raid default mode.</summary>
        public bool IsRaidDefault { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CharacterMode"/> class.</summary>
        public CharacterMode()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CharacterMode"/> class.</summary>
        /// <param name="id">The mode identifier.</param>
        public CharacterMode(string id)
        {
            Id = id;
        }
    }
}