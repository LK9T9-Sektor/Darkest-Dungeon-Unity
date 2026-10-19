namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Cause of an event: what the player or AI did to make it happen.</summary>
    /// <param name="Kind">The cause kind (see <see cref="GameInputKinds"/>).</param>
    /// <param name="Target">The target unit id, if any.</param>
    /// <param name="Skill">The skill id, if any.</param>
    /// <param name="Slot">The party/formation slot, if any.</param>
    public sealed record GameInput(string Kind, string? Target = null, string? Skill = null, string? Slot = null);
}
