namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A quest was selected on the estate quest board.</summary>
    /// <param name="QuestId">The quest id.</param>
    /// <param name="Dungeon">The dungeon id.</param>
    /// <param name="Type">The quest type id.</param>
    /// <param name="Difficulty">The quest difficulty.</param>
    /// <param name="Length">The quest length.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateQuestSelected(
        string QuestId,
        string Dungeon,
        string Type,
        int Difficulty,
        int Length,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
