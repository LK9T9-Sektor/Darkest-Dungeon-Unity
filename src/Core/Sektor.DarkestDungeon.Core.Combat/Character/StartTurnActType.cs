namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Start of turn action types.</summary>
    public enum StartTurnActType
    {
        Nothing, BarkStress,
        ChangePosition, IgnoreCommand,
        RandomCommand, RetreatFromCombat,
        AttackFriendly, AttackSelf,
        MarkSelf, StressHealSelf,
        StressHealParty, BuffAlly,
        BuffParty, HealSelf
    }
}
