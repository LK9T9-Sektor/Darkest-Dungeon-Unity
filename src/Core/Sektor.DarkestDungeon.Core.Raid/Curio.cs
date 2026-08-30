using System.Collections.Generic;
using System.IO;

namespace Sektor.DarkestDungeon.Core.Raid
{
    /// <summary>A curio prop with its interactions and item interactions.</summary>
    public class Curio : Prop
    {
        /// <summary>Gets the identifier of the non-tutorial counterpart of this curio.</summary>
        public string OriginalId
        {
            get
            {
                if (StringId == "tutorial_shovel")
                    return "unlocked_strongbox";
                else if (StringId == "tutorial_key")
                    return "discarded_pack";
                else if (StringId == "tutorial_holy")
                    return "sack";

                return StringId;
            }
        }

        /// <summary>Gets or sets a value indicating whether the curio is a full curio.</summary>
        public bool IsFullCurio { get; set; }

        /// <summary>Gets or sets a value indicating whether the curio is quest-related.</summary>
        public bool IsQuestCurio { get; set; }

        /// <summary>Gets or sets the result types available for the curio.</summary>
        public string ResultTypes { get; set; }

        /// <summary>Gets or sets the region where the curio can be found.</summary>
        public string RegionFound { get; set; }

        /// <summary>Gets the tags of the curio.</summary>
        public List<string> Tags { get; set; }

        /// <summary>Gets the interaction results of the curio.</summary>
        public List<CurioInteraction> Results { get; set; }

        /// <summary>Gets the item interactions of the curio.</summary>
        public List<ItemInteraction> ItemInteractions { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Curio"/> class.</summary>
        public Curio()
        {
            Type = AreaType.Curio;
            Tags = new List<string>();
            Results = new List<CurioInteraction>();
            ItemInteractions = new List<ItemInteraction>();
        }

        /// <summary>Initializes a new instance of the <see cref="Curio"/> class with the given id.</summary>
        /// <param name="id">The string identifier of the curio.</param>
        public Curio(string id) : this()
        {
            StringId = id;
        }

        /// <summary>Writes the curio state into the binary writer.</summary>
        /// <param name="bw">The binary writer receiving the serialized state.</param>
        public override void Write(System.IO.BinaryWriter bw)
        {
            base.Write(bw);

            bw.Write(IsQuestCurio);
            bw.Write(StringId);
        }
    }
}
