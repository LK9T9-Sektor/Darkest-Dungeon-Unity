namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>Exploration knowledge state of an area.</summary>
    public enum Knowledge
    {
        /// <summary>The area has not been explored.</summary>
        Hidden = 0,

        /// <summary>The area was scouted.</summary>
        Scouted,

        /// <summary>The area was visited.</summary>
        Visited,

        /// <summary>The area was completed.</summary>
        Completed,
    }
}