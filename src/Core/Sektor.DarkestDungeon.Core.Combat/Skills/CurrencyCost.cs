namespace Sektor.DarkestDungeon.Core.Combat.Skills
{
    /// <summary>Currency cost for camping skills.</summary>
    public class CurrencyCost
    {
        /// <summary>Gets or sets the currency type.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the amount.</summary>
        public int Amount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CurrencyCost"/> class.</summary>
        public CurrencyCost()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CurrencyCost"/> class.</summary>
        /// <param name="type">The currency type.</param>
        /// <param name="amount">The amount.</param>
        public CurrencyCost(string type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}
