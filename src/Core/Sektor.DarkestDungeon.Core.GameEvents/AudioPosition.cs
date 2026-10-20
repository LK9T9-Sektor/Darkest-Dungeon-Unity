namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A world position for a 3D audio event.</summary>
    /// <param name="X">X coordinate.</param>
    /// <param name="Y">Y coordinate.</param>
    /// <param name="Z">Z coordinate.</param>
    public sealed record AudioPosition(float X, float Y, float Z);
}
