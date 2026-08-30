namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>A trap prop placed on a hall sector.</summary>
    public class Trap : Prop
    {
        /// <summary>Initializes a new instance of the <see cref="Trap"/> class.</summary>
        /// <param name="id">The trap identifier.</param>
        public Trap(string id)
        {
            StringId = id;
            Type = AreaType.Trap;
        }
    }
}