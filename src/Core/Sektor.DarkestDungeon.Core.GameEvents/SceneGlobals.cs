namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Global session parameters of a scene snapshot.</summary>
    /// <param name="Torch">Torch value (0..100).</param>
    /// <param name="Gold">Party gold.</param>
    public sealed record SceneGlobals(int Torch, int Gold);
}
