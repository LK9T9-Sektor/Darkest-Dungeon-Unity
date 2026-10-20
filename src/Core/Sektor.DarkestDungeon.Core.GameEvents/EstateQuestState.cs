namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One quest available in the estate quest board.</summary>
    /// <param name="Id">The quest id.</param>
    /// <param name="Dungeon">The dungeon id.</param>
    /// <param name="Type">The quest type id.</param>
    /// <param name="Difficulty">The quest difficulty.</param>
    /// <param name="Length">The quest length.</param>
    /// <param name="GoalId">The primary goal id.</param>
    /// <param name="IsPlot">Whether the quest is a plot quest.</param>
    public sealed record EstateQuestState(
        string Id,
        string Dungeon,
        string Type,
        int Difficulty,
        int Length,
        string GoalId,
        bool IsPlot);
}
