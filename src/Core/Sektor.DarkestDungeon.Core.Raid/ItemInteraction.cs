namespace Sektor.DarkestDungeon.Core.Raid
{
    /// <summary>An interaction triggered by using an item on a curio.</summary>
    public class ItemInteraction : CurioInteraction
    {
        /// <summary>Gets or sets the item id used for the interaction.</summary>
        public string ItemId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="ItemInteraction"/> class.</summary>
        public ItemInteraction()
        {
            Chance = 1;
        }

        /// <summary>Initializes a new instance of the <see cref="ItemInteraction"/> class.</summary>
        /// <param name="chance">The selection chance.</param>
        /// <param name="itemId">The item id.</param>
        /// <param name="resultType">The result type.</param>
        public ItemInteraction(int chance, string itemId, string resultType)
        {
            Chance = chance;
            ItemId = itemId;
            ResultType = resultType;
        }

        /// <summary>Gets the display string of the interaction.</summary>
        /// <returns>The display string.</returns>
        public override string ResultString()
        {
            return ItemId + "_" + ResultType;
        }
    }
}
