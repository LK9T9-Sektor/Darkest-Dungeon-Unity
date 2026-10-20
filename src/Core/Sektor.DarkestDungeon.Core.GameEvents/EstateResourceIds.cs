using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Well-known estate resource ids used by snapshots and costs.</summary>
    public static class EstateResourceIds
    {
        /// <summary>Gold.</summary>
        public const string Gold = "gold";

        /// <summary>Bust heirlooms.</summary>
        public const string Bust = "bust";

        /// <summary>Portrait heirlooms.</summary>
        public const string Portrait = "portrait";

        /// <summary>Deed heirlooms.</summary>
        public const string Deed = "deed";

        /// <summary>Crest heirlooms.</summary>
        public const string Crest = "crest";

        /// <summary>All estate resources in display order.</summary>
        public static IReadOnlyList<string> All { get; } = new[]
        {
            Gold,
            Bust,
            Portrait,
            Deed,
            Crest,
        };
    }
}
