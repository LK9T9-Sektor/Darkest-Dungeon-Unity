using System.IO;

namespace Sektor.DarkestDungeon.Core.Save
{
    /// <summary>
    /// Contract for domain objects that can serialize their state into the binary save stream.
    /// Shared by the content models (e.g. raid props) and the presentation save codec.
    /// </summary>
    public interface IBinarySaveData
    {
        /// <summary>Gets a value indicating whether this instance must be persisted.</summary>
        bool IsMeetingSaveCriteria { get; }

        /// <summary>Writes the instance state into the binary writer.</summary>
        /// <param name="bw">The binary writer receiving the serialized state.</param>
        void Write(BinaryWriter bw);

        /// <summary>Reads the instance state from the binary reader.</summary>
        /// <param name="br">The binary reader providing the serialized state.</param>
        void Read(BinaryReader br);
    }
}
