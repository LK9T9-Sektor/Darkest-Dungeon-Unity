namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Turn flow phase of a duel. Both sides build identical formations; the host owns Heroes, the client owns Monsters.</summary>
    public enum DuelPhase
    {
        /// <summary>The duel has not been started.</summary>
        NotStarted,

        /// <summary>A hero of the host's party is acting.</summary>
        WaitingForHostAction,

        /// <summary>A monster (client hero) is acting.</summary>
        WaitingForClientAction,

        /// <summary>The duel has finished.</summary>
        Finished
    }
}