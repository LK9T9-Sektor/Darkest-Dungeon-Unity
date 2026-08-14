using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Raid
{
    /// <summary>A weighted interaction result of a curio.</summary>
    public class CurioInteraction : IProportionValue
    {
        /// <summary>Gets or sets the result type.</summary>
        public string ResultType { get; set; }

        /// <summary>Gets the concrete results of the interaction.</summary>
        public List<CurioResult> Results { get; set; }

        /// <summary>Gets or sets the selection chance of the interaction.</summary>
        public int Chance { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CurioInteraction"/> class.</summary>
        public CurioInteraction()
        {
            Results = new List<CurioResult>();
        }

        /// <summary>Initializes a new instance of the <see cref="CurioInteraction"/> class.</summary>
        /// <param name="chance">The selection chance.</param>
        /// <param name="resultType">The result type.</param>
        public CurioInteraction(int chance, string resultType) : this()
        {
            Chance = chance;
            ResultType = resultType;
        }

        /// <summary>Gets the display string of the interaction.</summary>
        /// <returns>The display string.</returns>
        public virtual string ResultString()
        {
            if (ResultType == "scouting")
                return "scout";
            else
                return ResultType;
        }
    }
}
