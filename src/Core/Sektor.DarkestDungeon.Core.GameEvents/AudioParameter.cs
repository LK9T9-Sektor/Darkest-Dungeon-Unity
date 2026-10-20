namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One FMOD parameter applied before an audio event starts.</summary>
    /// <param name="Name">The parameter name.</param>
    /// <param name="Value">The numeric value.</param>
    /// <param name="Label">The parameter label, when the parameter is labelled.</param>
    public sealed record AudioParameter(
        string Name,
        float Value,
        string? Label = null);
}
