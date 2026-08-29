using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Content.Raid
{
    /// <summary>A single weighted curio result item.</summary>
    public class CurioResult : IProportionValue
    {
        /// <summary>Gets or sets the item id granted by the result.</summary>
        public string Item { get; set; }

        /// <summary>Gets or sets the number of draws of the result.</summary>
        public int Draws { get; set; }

        /// <summary>Gets or sets a value indicating whether the result is combined into the parent draw.</summary>
        public bool IsCombined { get; set; }

        /// <summary>Gets or sets the selection chance of the result.</summary>
        public int Chance { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CurioResult"/> class.</summary>
        public CurioResult()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CurioResult"/> class.</summary>
        /// <param name="chance">The selection chance.</param>
        /// <param name="draws">The number of draws.</param>
        /// <param name="item">The item id.</param>
        public CurioResult(int chance, int draws, string item)
        {
            Chance = chance;
            Draws = draws;
            Item = item;
        }
    }
}
