namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>An obstacle prop placed on a hall sector.</summary>
    public class Obstacle : Prop
    {
        /// <summary>Initializes a new instance of the <see cref="Obstacle"/> class.</summary>
        /// <param name="id">The obstacle identifier.</param>
        public Obstacle(string id)
        {
            StringId = id;
            Type = AreaType.Obstacle;
        }
    }
}