using System.IO;

using Sektor.DarkestDungeon.Core.Save;

namespace Sektor.DarkestDungeon.Core.Content.Raid
{
    /// <summary>
    /// Base class for placeable raid props (curios, obstacles, traps, doors). Holds the string
    /// identifier and the area type and supports binary save serialization.
    /// </summary>
    public abstract class Prop : IBinarySaveData
    {
        /// <summary>Gets or sets the string identifier of the prop.</summary>
        public string StringId { get; set; }

        /// <summary>Gets the area type of the prop.</summary>
        public AreaType Type { get; protected set; }

        /// <summary>Gets a value indicating whether the prop is always persisted.</summary>
        public bool IsMeetingSaveCriteria { get { return true; } }

        /// <summary>Writes the prop state into the binary writer.</summary>
        /// <param name="bw">The binary writer receiving the serialized state.</param>
        public virtual void Write(BinaryWriter bw)
        {
            bw.Write((int)Type);
        }

        /// <summary>Reads the prop state from the binary reader.</summary>
        /// <param name="br">The binary reader providing the serialized state.</param>
        public virtual void Read(BinaryReader br)
        {
            // Type is resolved by the save codec before Read is invoked.
        }
    }
}
